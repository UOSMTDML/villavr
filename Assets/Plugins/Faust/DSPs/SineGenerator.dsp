declare filename "SineGenerator.dsp"; declare name "SineGenerator"; declare version "2.54.11";
declare compile_options "-single -scal -I libraries/ -I project/ -lang wasm";
declare library_path0 "/libraries/stdfaust.lib";
declare library_path1 "/libraries/oscillators.lib";
declare library_path2 "/libraries/platform.lib";
declare library_path3 "/libraries/maths.lib";
declare library_path4 "/project/basics.lib";
declare library_path5 "/libraries/signals.lib";
declare author "Grame";
declare basics_lib_name "Faust Basic Element Library";
declare basics_lib_version "0.9";
declare copyright "(c)GRAME 2009";
declare filename "FaustDSP";
declare license "BSD";
declare maths_lib_author "GRAME";
declare maths_lib_copyright "GRAME";
declare maths_lib_license "LGPL with exception";
declare maths_lib_name "Faust Math Library";
declare maths_lib_version "2.5";
declare name "osci";
declare oscillators_lib_name "Faust Oscillator Library";
declare oscillators_lib_version "0.3";
declare platform_lib_name "Generic Platform Library";
declare platform_lib_version "0.3";
declare signals_lib_name "Faust Signal Routing Library";
declare signals_lib_version "0.3";
declare version "1.0";
ID_0 = (65536 : int);
ID_1 = _, ID_0;
ID_2 = (ID_1 : %);
ID_3 = (1 : mem);
ID_4 = _, ID_3;
ID_5 = (ID_4 : +);
ID_6 = ID_2 ~ ID_5;
ID_7 = (ID_6 : float);
ID_8 = ID_7, 6.2831855f;
ID_9 = (ID_8 : *);
ID_10 = (65536 : float);
ID_11 = ID_9, ID_10;
ID_12 = ID_11 : /;
ID_13 = (ID_12 : sin);
ID_14 = hslider("freq [unit:Hz]", 1e+03f, 2e+01f, 2.4e+04f, 1.0f);
ID_15 = fconstant(int fSamplingFreq, <math.h>);
ID_16 = 1.0f, ID_15;
ID_17 = (ID_16 : max);
ID_18 = 1.92e+05f, ID_17;
ID_19 = (ID_18 : min);
ID_20 = ID_14, ID_19;
ID_21 = (ID_20 : /);
ID_22 = _, ID_21;
ID_23 = (ID_22 : +);
ID_24 = ID_23, 0;
ID_25 = 0, ID_24;
ID_26 = ID_25 : select2;
ID_27 = (ID_26 : \(x1).(x1,(x1 : floor) : -));
ID_28 = ID_27 ~ _;
ID_29 = _, ID_10;
ID_30 = ID_29 : *;
ID_31 = ID_28 : ID_30;
ID_32 = (ID_31 : int);
ID_33 = ID_13, ID_32;
ID_34 = 65537, ID_33;
ID_35 = (ID_34 : rdtable);
ID_36 = (ID_26 : \(x2).(x2,(x2 : floor) : -));
ID_37 = ID_36 ~ _;
ID_38 = _, 65536.0f;
ID_39 = ID_38 : *;
ID_40 = (ID_37 : ID_39);
ID_41 = (ID_40 : floor);
ID_42 = ID_40, ID_41;
ID_43 = (ID_42 : -);
ID_44 = ID_32, 1;
ID_45 = (ID_44 : +);
ID_46 = ID_13, ID_45;
ID_47 = 65537, ID_46;
ID_48 = (ID_47 : rdtable);
ID_49 = ID_48, ID_35;
ID_50 = (ID_49 : -);
ID_51 = ID_43, ID_50;
ID_52 = (ID_51 : *);
ID_53 = ID_35, ID_52;
ID_54 = (ID_53 : +);
ID_55 = hslider("volume [unit:dB]", 0.0f, -96.0f, 0.0f, 0.1f);
ID_56 = \(x3).(1e+01f,(x3,2e+01f : /) : pow) : \(x4).(\(x5).(((1.0f,(1,(44.1f,(1.92e+05f,(1.0f,fconstant(int fSamplingFreq, <math.h>) : max) : min) : /) : -) : -),x4 : *),((1,(44.1f,(1.92e+05f,(1.0f,fconstant(int fSamplingFreq, <math.h>) : max) : min) : /) : -),x5 : *) : +)~_);
ID_57 = (ID_55 : ID_56);
ID_58 = ID_54, ID_57;
ID_59 = ID_58 : *;
ID_60 = vgroup("Oscillator", ID_59);
process = ID_60;
