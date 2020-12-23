#ifndef _SEARCHATHING_SCURVE_STEPPER_EXAMPLE_H
#define _SEARCHATHING_SCURVE_STEPPER_EXAMPLE_H

#include <mbed.h>

#define EXAMPLE02 // set example to run in debug

#if defined(STM32F767xx)
#define M1_PIN PinName::PF_1
#define M2_PIN PinName::PF_0
#define M3_PIN PinName::PD_0
#define M4_PIN PinName::PD_1
#define M5_PIN PinName::PG_0
#define M6_PIN PinName::PE_6
#define M7_PIN PinName::PE_3
#define M8_PIN PinName::PF_8
#define M9_PIN PinName::PF_7
#define M10_PIN PinName::PF_9
#elif defined(STM32F446xx)
#define M1_PIN PinName::PC_10
#define M2_PIN PinName::PC_12
#define M3_PIN PinName::PC_12
#define M4_PIN PinName::PC_12
#define M5_PIN PinName::PC_12
#define M6_PIN PinName::PC_12
#define M7_PIN PinName::PC_12
#define M8_PIN PinName::PC_12
#define M9_PIN PinName::PC_12
#define M10_PIN PinName::PC_12
#endif

#endif
