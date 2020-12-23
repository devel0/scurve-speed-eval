#ifndef UNIT_TEST
#include "example.h"

#ifdef EXAMPLE01

// if got ERROR during execution it may you need to run in release mode (ie. upload without debug)
// because in debug mode there are lack of optimizations

#include <mbed.h>

#include <scurve-impulse-stepper.h>
#include <timer-utils.h>
#include <slist.h>
#include <sstream>

DigitalOut mPort(M1_PIN);

int main()
{
    auto wait_time = 2000ms;
    auto motion_time = 390ms;
    auto pulse_rev = 400;
    auto pulse_width = 5us;
    auto rev_cnt = 1.5;

    //---

    Timer timer;    

    SCurveImpulseStepper m(1, timer, mPort, pulse_rev, pulse_width);

    timer.start();

    bool nfoReported = false;

    while (true)
    {
        auto t_now = timer.elapsed_time();

        auto m_state = m.state();

        if (m_state == SCurveStepperMotorState::idle && t_now - m.motionEnd() > wait_time ||
            m_state == SCurveStepperMotorState::unknown)
        {
            m.move(rev_cnt, motion_time);
        }

        if (m_state == SCurveStepperMotorState::moving)
        {
            nfoReported = false;
        }

        if (m_state == SCurveStepperMotorState::idle && !nfoReported)
        {
            m.debugStats();

            nfoReported = true;
        }

        m.control();        
    }
}

#endif
#endif
