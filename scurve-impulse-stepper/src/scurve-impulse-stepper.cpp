#include <scurve-impulse-stepper.h>

#include <cmath>
#include <timer-utils.h>
#include <constant-utils.h>

SCurveImpulseStepper::SCurveImpulseStepper(int __tag,
                                           Timer &_timer,
                                           DigitalOut &_pulse_pin,
                                           int _pulse_rev,
                                           std::chrono::microseconds _pulse_width_min) : timer(_timer), pulse_pin(_pulse_pin)

{
    _tag = __tag;
    pulse_rev = _pulse_rev;
    pulse_width_min = _pulse_width_min;
    pulse_total = 0;

    motion_count = 0;
    _state = SCurveStepperMotorState::unknown;
    _4PI = 4 * PI;
    _8PI2 = 8 * PI * PI;
}

void SCurveImpulseStepper::move(double revCount, std::chrono::microseconds duration)
{
    if (_state == SCurveStepperMotorState::idle || _state == SCurveStepperMotorState::unknown)
    {
        auto cur_t = timer.elapsed_time();

        d = duration;
        d_us = chrono_us(d);
        half_d_us = d_us / 2;
        s_us = 2 * revCount / d_us;
        current_period_us = d_us;
        current_period_start = cur_t;

        if (_state == SCurveStepperMotorState::unknown) // init
        {
            period_min_us = d_us;
        }

        _state = SCurveStepperMotorState::moving;
        motion_start = timer.elapsed_time();
        pulse_expected = 0;
        pulse_executed = 0;
        pulse_total = round(revCount * pulse_rev);
        pulse_excees = 0;
    }
}

void SCurveImpulseStepper::pulseDownFn()
{
    pulse_pin = 0;
}

void SCurveImpulseStepper::control()
{
    if (_state == SCurveStepperMotorState::idle)
        return;

    auto cur_t = timer.elapsed_time();

    double cur_t_us = chrono_us(cur_t);
    double t_us = cur_t_us - chrono_us(motion_start);

    bool time_exceeded = t_us > d_us;

    if (t_us < half_d_us) // acceleration phase
    {
        current_speed_ps =
            (2 * s_us / d_us * (t_us - d_us * sin(_4PI * t_us / d_us) / (_4PI))) * pulse_rev;

        pulse_expected = (int)round(
            (s_us * d_us * (cos(_4PI * t_us / d_us) - 1) / _8PI2 + (s_us * t_us * t_us / d_us)) * pulse_rev);
    }
    else // deceleration phase
    {
        t_us -= half_d_us;

        current_speed_ps =
            (2 * s_us * sin(_4PI * t_us / d_us) / _4PI - 2 * s_us * t_us / d_us + s_us) * pulse_rev;
        double kk = cos(_4PI * t_us / d_us);
        pulse_expected = (int)round(
            (s_us * d_us * (1 - kk) / _8PI2 - s_us * t_us * t_us / d_us + s_us * t_us + s_us * d_us / 4.0) * pulse_rev);
    }

    if (current_speed_ps == 0)
        current_period_us = d_us;
    else
        current_period_us = 1.0 / current_speed_ps;

    if (pulse_pin != 1 && (pulse_executed < pulse_expected))
    {
        auto now_period_us = chrono_us(cur_t - current_period_start);
        // adjust
        double rp = current_period_us;
        if (pulse_executed < pulse_expected)
            rp /= 2;
        if (now_period_us >= rp)
        {
            if (time_exceeded)
                ++pulse_excees;

            if (pulse_excees > 1)
            {
                motion_end = cur_t;
                _state = SCurveStepperMotorState::failed;
                ++motion_count;
                return;
            }

            if (now_period_us < period_min_us)
            {
                period_min_us = now_period_us;
            }

            current_period_start = cur_t;
            pulse_pin = 1;
            pulse_down.attach(callback(this, &SCurveImpulseStepper::pulseDownFn), pulse_width_min);
            ++pulse_executed;
        }
    }

    // adjust ensure executed finished
    if (pulse_executed >= pulse_expected && cur_t - motion_start > d)
    {
        motion_end = cur_t;
        _state = SCurveStepperMotorState::idle;
    }
}

void SCurveImpulseStepper::debugStats(bool block_on_error)
{
    auto effective_motion_duration = motion_end - motion_start;

    uint64_t mdur_ms = chrono_ms(effective_motion_duration);

    printf("m[%d] pulse_executed: %d   pulse_expected: %d   motion_dur: %llums   period_min: %lluus\n",
           _tag,
           pulse_executed,
           pulse_expected,
           mdur_ms,
           period_min_us);

    if (block_on_error && pulse_executed != pulse_expected)
    {
        printf("ERROR\n");
        while (1)
            ;
    }
}