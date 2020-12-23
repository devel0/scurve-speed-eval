#ifndef _SCURVE_STEPPER_H
#define _SCURVE_STEPPER_H

#include <mbed.h>

enum SCurveStepperMotorState
{
    unknown,
    idle,
    moving,
    failed
};

class SCurveImpulseStepper
{
private:
    int _tag;
    SCurveStepperMotorState _state;
    Timer &timer;
    DigitalOut &pulse_pin;
    std::chrono::microseconds pulse_width_min;

    std::chrono::microseconds motion_start;
    std::chrono::microseconds motion_end;
    uint64_t period_min_us;
    int pulse_rev;
    int pulse_expected;
    int pulse_executed;
    int pulse_total;
    Timeout pulse_down;
    int motion_count;
    int pulse_excees;

    std::chrono::microseconds d;
    double d_us;
    double half_d_us;
    double s_us;
    double current_period_us;
    double current_speed_ps;
    std::chrono::microseconds current_period_start;

    double _4PI;
    double _8PI2;

    void pulseDownFn();

public:
    int tag() const { return _tag; }
    SCurveStepperMotorState state() const { return _state; }
    std::chrono::microseconds motionStart() const { return motion_start; }
    std::chrono::microseconds motionEnd() const { return motion_end; }
    /// min period measured
    std::chrono::microseconds periodMin() const { return std::chrono::microseconds(period_min_us); }
    int pulseRev() const { return pulse_rev; }
    double currentSpeedPulseSec() const { return current_speed_ps; }
    double currentSpeedRevSec() const { return current_speed_ps / pulse_rev; }
    /// increase when stepper goes idle from a motion state
    int motionCount() const { return motion_count; }
    /// pulse executed last motion
    int pulseExecuted() const { return pulse_executed; }
    /// pulse expected last motion
    int pulseExpected() const { return pulse_expected; }

    SCurveImpulseStepper(int __tag, Timer &_timer, DigitalOut &_pulsePin, int _pulse_rev,
                  std::chrono::microseconds _pulse_width_min = 20us);

    void move(double revCount, std::chrono::microseconds duration);    

    void control();

    void debugStats(bool block_on_error = true);
};

#endif
