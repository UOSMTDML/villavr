declare filename "LowPassFilter.dsp"; declare name "LowPassFilter"; declare version "2.54.11";
declare compile_options "-single -scal -I libraries/ -I project/ -lang wasm";
declare library_path0 "/libraries/maxmsp.lib";
declare library_path1 "/libraries/maths.lib";
declare library_path2 "/libraries/platform.lib";
declare filename "FaustDSP";
declare maths_lib_author "GRAME";
declare maths_lib_copyright "GRAME";
declare maths_lib_license "LGPL with exception";
declare maths_lib_name "Faust Math Library";
declare maths_lib_version "2.5";
declare maxmsp_lib_author "GRAME";
declare maxmsp_lib_copyright "GRAME";
declare maxmsp_lib_license "LGPL with exception";
declare maxmsp_lib_name "MaxMSP compatibility Library";
declare maxmsp_lib_version "1.1";
declare name "LPF";
declare platform_lib_name "Generic Platform Library";
declare platform_lib_version "0.3";
process = \(x1).(x1,(((1,((6.2831855f,(0,hslider("Freq", 1e+03f, 1e+02f, 1e+04f, 1.0f) : max) : *),(1.92e+05f,(1.0f,fconstant(int fSamplingFreq, <math.h>) : max) : min) : / : cos) : -),2 : /),(1,(((6.2831855f,(0,hslider("Freq", 1e+03f, 1e+02f, 1e+04f, 1.0f) : max) : *),(1.92e+05f,(1.0f,fconstant(int fSamplingFreq, <math.h>) : max) : min) : / : sin),(2,(0.001f,hslider("Q", 1.0f, 0.01f, 1e+02f, 0.01f) : max) : *) : /) : +) : /),((1,((6.2831855f,(0,hslider("Freq", 1e+03f, 1e+02f, 1e+04f, 1.0f) : max) : *),(1.92e+05f,(1.0f,fconstant(int fSamplingFreq, <math.h>) : max) : min) : / : cos) : -),(1,(((6.2831855f,(0,hslider("Freq", 1e+03f, 1e+02f, 1e+04f, 1.0f) : max) : *),(1.92e+05f,(1.0f,fconstant(int fSamplingFreq, <math.h>) : max) : min) : / : sin),(2,(0.001f,hslider("Q", 1.0f, 0.01f, 1e+02f, 0.01f) : max) : *) : /) : +) : /),(((1,((6.2831855f,(0,hslider("Freq", 1e+03f, 1e+02f, 1e+04f, 1.0f) : max) : *),(1.92e+05f,(1.0f,fconstant(int fSamplingFreq, <math.h>) : max) : min) : / : cos) : -),2 : /),(1,(((6.2831855f,(0,hslider("Freq", 1e+03f, 1e+02f, 1e+04f, 1.0f) : max) : *),(1.92e+05f,(1.0f,fconstant(int fSamplingFreq, <math.h>) : max) : min) : / : sin),(2,(0.001f,hslider("Q", 1.0f, 0.01f, 1e+02f, 0.01f) : max) : *) : /) : +) : /),((-2,((6.2831855f,(0,hslider("Freq", 1e+03f, 1e+02f, 1e+04f, 1.0f) : max) : *),(1.92e+05f,(1.0f,fconstant(int fSamplingFreq, <math.h>) : max) : min) : / : cos) : *),(1,(((6.2831855f,(0,hslider("Freq", 1e+03f, 1e+02f, 1e+04f, 1.0f) : max) : *),(1.92e+05f,(1.0f,fconstant(int fSamplingFreq, <math.h>) : max) : min) : / : sin),(2,(0.001f,hslider("Q", 1.0f, 0.01f, 1e+02f, 0.01f) : max) : *) : /) : +) : /),((1,(((6.2831855f,(0,hslider("Freq", 1e+03f, 1e+02f, 1e+04f, 1.0f) : max) : *),(1.92e+05f,(1.0f,fconstant(int fSamplingFreq, <math.h>) : max) : min) : / : sin),(2,(0.001f,hslider("Q", 1.0f, 0.01f, 1e+02f, 0.01f) : max) : *) : /) : -),(1,(((6.2831855f,(0,hslider("Freq", 1e+03f, 1e+02f, 1e+04f, 1.0f) : max) : *),(1.92e+05f,(1.0f,fconstant(int fSamplingFreq, <math.h>) : max) : min) : / : sin),(2,(0.001f,hslider("Q", 1.0f, 0.01f, 1e+02f, 0.01f) : max) : *) : /) : +) : /) : \(x2).(\(x3).(\(x4).(\(x5).(\(x6).(\(x7).(x2 : +~(-1,\(x8).((x6,x8 : *),(x7,(x8 : mem) : *) : +) : *) : \(x9).(((x3,x9 : *),(x4,(x9 : mem) : *) : +),(x5,(x9 : mem : mem) : *) : +))))))));
