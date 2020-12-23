#ifndef UNIT_TEST
#include "example.h"

#ifdef EXAMPLE02

// if got ERROR during execution it may you need to run in release mode (ie. upload without debug)
// because in debug mode there are lack of optimizations

#include <mbed.h>

#include <scurve-impulse-stepper.h>
#include <timer-utils.h>
#include <slist.h>
#include <sstream>

PinName m_pins[] = {M1_PIN, M2_PIN, M3_PIN, M4_PIN, M5_PIN, M6_PIN, M7_PIN, M8_PIN, M9_PIN, M10_PIN};
double m_rev_cnt[] = {3, 2.5, 2, 1.5, 1, 0.5, 0.25, 0.5, 3, 1};

int main()
{
    auto wait_time = 2000ms;
    auto motion_time = 390ms;
    auto pulse_rev = 400;
    auto screen_debug = true;
    auto pulse_width = 5us;

    //---

    Timer timer;

    SList<SCurveImpulseStepper *> motors;

    int motors_count = sizeof(m_pins) / sizeof(PinName);
    for (int i = 0; i < motors_count; ++i)
    {
        auto port = new DigitalOut(m_pins[i]);
        auto tag = i + 1;
        motors.Add(new SCurveImpulseStepper(tag, timer, *port, pulse_rev, pulse_width));
    }

    timer.start();

    bool nfoReported = false;

    auto firstMotorNode = motors.GetNode(0);

    while (true)
    {
        auto t_now = timer.elapsed_time();

        bool allIdle = true;

        auto n = firstMotorNode;
        int mi = 0;
        while (n != NULL)
        {
            SCurveImpulseStepper &m = *(n->data);
            auto m_state = m.state();

            if (m_state == SCurveStepperMotorState::idle && t_now - m.motionEnd() > wait_time ||
                m_state == SCurveStepperMotorState::unknown)
            {
                if (m_state == SCurveStepperMotorState::unknown)
                    allIdle = false;
                m.move(m_rev_cnt[mi], motion_time);
            }

            if (m_state == SCurveStepperMotorState::moving)
            {
                nfoReported = false;
                allIdle = false;
            }
            n = n->next;
            ++mi;
        }

        if (allIdle && !nfoReported)
        {
            int mnr = 1;
            n = firstMotorNode;
            while (n != NULL)
            {
                SCurveImpulseStepper &m = *(n->data);
                m.debugStats();

                mnr++;
                n = n->next;
            }
            nfoReported = true;
        }

        n = firstMotorNode;
        while (n != NULL)
        {
            n->data->control();
            n = n->next;
        }
    }
}

#endif
#endif
