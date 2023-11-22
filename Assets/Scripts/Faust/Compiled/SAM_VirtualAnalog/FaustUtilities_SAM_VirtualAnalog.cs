using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/* @brief This file contains additional code to communicate with the plugin and build the inspector interface
* @brief It shouldn't be changed
*/
namespace FaustUtilities_SAM_VirtualAnalog {

	#if UNITY_EDITOR
    using UnityEditor;

	[CustomEditor(typeof(FaustPlugin_SAM_VirtualAnalog))]
	public class FaustPlugin_Editor: Editor {

		// Initialization of the dsp
		private FaustPlugin_SAM_VirtualAnalog _dsp;

		// Initialization of the UI related variables
		private string fJSON;
		private FaustUI fUI;
		private int param;

		// @brief All instantiations have to be done in the OnEnable() method.
		private void OnEnable() {
			fJSON = "{	\"name\": \"virtualAnalog\",	\"filename\": \"virtualAnalog.dsp\",	\"version\": \"2.37.3\",	\"compile_options\": \"-lang cpp -es 1 -single -ftz 0\",	\"library_list\": [\"/home/mtdml/Desktop/New Folder/layout2.dsp\",\"/usr/share/faust/stdfaust.lib\",\"/usr/share/faust/noises.lib\",\"/usr/share/faust/filters.lib\",\"/usr/share/faust/maths.lib\",\"/usr/share/faust/basics.lib\",\"/usr/share/faust/signals.lib\",\"/usr/share/faust/platform.lib\",\"/usr/share/faust/oscillators.lib\",\"/usr/share/faust/envelopes.lib\",\"/usr/share/faust/vaeffects.lib\"],	\"include_pathnames\": [\"/usr/share/faust\",\"/usr/local/share/faust\",\"/usr/share/faust\",\".\",\"/home/mtdml/Desktop/New Folder/.\",\"/home/mtdml/Desktop/New Folder\"],	\"inputs\": 1,	\"outputs\": 2,	\"meta\": [ 		{ \"basics.lib/name\": \"Faust Basic Element Library\" },		{ \"basics.lib/version\": \"0.2\" },		{ \"compile_options\": \"-lang cpp -es 1 -single -ftz 0\" },		{ \"envelopes.lib/author\": \"GRAME\" },		{ \"envelopes.lib/copyright\": \"GRAME\" },		{ \"envelopes.lib/license\": \"LGPL with exception\" },		{ \"envelopes.lib/name\": \"Faust Envelope Library\" },		{ \"envelopes.lib/version\": \"0.1\" },		{ \"filename\": \"virtualAnalog.dsp\" },		{ \"filters.lib/allpassnnlt:author\": \"Julius O. Smith III\" },		{ \"filters.lib/allpassnnlt:copyright\": \"Copyright (C) 2003-2019 by Julius O. Smith III <jos@ccrma.stanford.edu>\" },		{ \"filters.lib/allpassnnlt:license\": \"MIT-style STK-4.3 license\" },		{ \"filters.lib/fir:author\": \"Julius O. Smith III\" },		{ \"filters.lib/fir:copyright\": \"Copyright (C) 2003-2019 by Julius O. Smith III <jos@ccrma.stanford.edu>\" },		{ \"filters.lib/fir:license\": \"MIT-style STK-4.3 license\" },		{ \"filters.lib/iir:author\": \"Julius O. Smith III\" },		{ \"filters.lib/iir:copyright\": \"Copyright (C) 2003-2019 by Julius O. Smith III <jos@ccrma.stanford.edu>\" },		{ \"filters.lib/iir:license\": \"MIT-style STK-4.3 license\" },		{ \"filters.lib/lowpass0_highpass1\": \"Copyright (C) 2003-2019 by Julius O. Smith III <jos@ccrma.stanford.edu>\" },		{ \"filters.lib/name\": \"Faust Filters Library\" },		{ \"filters.lib/pole:author\": \"Julius O. Smith III\" },		{ \"filters.lib/pole:copyright\": \"Copyright (C) 2003-2019 by Julius O. Smith III <jos@ccrma.stanford.edu>\" },		{ \"filters.lib/pole:license\": \"MIT-style STK-4.3 license\" },		{ \"filters.lib/tf2np:author\": \"Julius O. Smith III\" },		{ \"filters.lib/tf2np:copyright\": \"Copyright (C) 2003-2019 by Julius O. Smith III <jos@ccrma.stanford.edu>\" },		{ \"filters.lib/tf2np:license\": \"MIT-style STK-4.3 license\" },		{ \"filters.lib/version\": \"0.3\" },		{ \"interface\": \"SmartKeyboard{     'Number of Keyboards':'2',     'Keyboard 0 - Number of Keys':'13',     'Keyboard 1 - Number of Keys':'13',     'Keyboard 0 - Lowest Key':'72',     'Keyboard 1 - Lowest Key':'60' }\" },		{ \"layout2.dsp/designer\": \"Robert A. Moog\" },		{ \"maths.lib/author\": \"GRAME\" },		{ \"maths.lib/copyright\": \"GRAME\" },		{ \"maths.lib/license\": \"LGPL with exception\" },		{ \"maths.lib/name\": \"Faust Math Library\" },		{ \"maths.lib/version\": \"2.5\" },		{ \"name\": \"virtualAnalog\" },		{ \"noises.lib/name\": \"Faust Noise Generator Library\" },		{ \"noises.lib/version\": \"0.1\" },		{ \"oscillators.lib/name\": \"Faust Oscillator Library\" },		{ \"oscillators.lib/version\": \"0.1\" },		{ \"platform.lib/name\": \"Generic Platform Library\" },		{ \"platform.lib/version\": \"0.2\" },		{ \"signals.lib/name\": \"Faust Signal Routing Library\" },		{ \"signals.lib/version\": \"0.1\" },		{ \"vaeffects.lib/moog_vcf_2bn:author\": \"Julius O. Smith III\" },		{ \"vaeffects.lib/moog_vcf_2bn:copyright\": \"Copyright (C) 2003-2019 by Julius O. Smith III <jos@ccrma.stanford.edu>\" },		{ \"vaeffects.lib/moog_vcf_2bn:license\": \"MIT-style STK-4.3 license\" },		{ \"vaeffects.lib/name\": \"Faust Virtual Analog Filter Effect Library\" },		{ \"vaeffects.lib/version\": \"0.2\" }	],	\"ui\": [ 		{			\"type\": \"hgroup\",			\"label\": \"virtualAnalog\",			\"items\": [ 				{					\"type\": \"vgroup\",					\"label\": \"Minimoog\",					\"meta\": [						{ \"0\": \"\" }					],					\"items\": [ 						{							\"type\": \"hgroup\",							\"label\": \"0x00\",							\"meta\": [								{ \"0\": \"\" }							],							\"items\": [ 								{									\"type\": \"vgroup\",									\"label\": \"Controllers\",									\"meta\": [										{ \"0\": \"\" }									],									\"items\": [ 										{											\"type\": \"hgroup\",											\"label\": \"Master Volume\",											\"meta\": [												{ \"0\": \"\" }											],											\"items\": [ 												{													\"type\": \"vslider\",													\"label\": \"MasterVolume\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Controllers/Master Volume/MasterVolume\",													\"meta\": [														{ \"midi\": \"ctrl 7\" },														{ \"style\": \"knob\" },														{ \"tooltip\": \"master volume, MIDI controlled\" }													],													\"init\": 0.7,													\"min\": 0,													\"max\": 1,													\"step\": 0.001												}											]										},										{											\"type\": \"hgroup\",											\"label\": \"Oscillator Tuning RELEVANTJSONFILE Switching\",											\"meta\": [												{ \"1\": \"\" }											],											\"items\": [ 												{													\"type\": \"vslider\",													\"label\": \"Tune\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Controllers/Oscillator Tuning RELEVANTJSONFILE Switching/Tune\",													\"meta\": [														{ \"0\": \"\" },														{ \"midi\": \"ctrl 47\" },														{ \"style\": \"knob\" },														{ \"tooltip\": \"Frequency-shift up or down for all oscillators in Octaves\" },														{ \"unit\": \"Octaves\" }													],													\"init\": 0,													\"min\": -1,													\"max\": 1,													\"step\": 0.001												},												{													\"type\": \"vgroup\",													\"label\": \"Switches\",													\"meta\": [														{ \"1\": \"\" }													],													\"items\": [ 														{															\"type\": \"vslider\",															\"label\": \"Osc. Mod.\",															\"address\": \"/virtualAnalog/Minimoog/0x00/Controllers/Oscillator Tuning RELEVANTJSONFILE Switching/Switches/Osc. Mod.\",															\"meta\": [																{ \"0\": \"\" },																{ \"color\": \"red\" },																{ \"midi\": \"ctrl 22\" },																{ \"style\": \"knob\" }															],															\"init\": 1,															\"min\": 0,															\"max\": 1,															\"step\": 1														},														{															\"type\": \"vslider\",															\"label\": \"Osc. 3 Ctl\",															\"address\": \"/virtualAnalog/Minimoog/0x00/Controllers/Oscillator Tuning RELEVANTJSONFILE Switching/Switches/Osc. 3 Ctl\",															\"meta\": [																{ \"1\": \"\" },																{ \"color\": \"red\" },																{ \"midi\": \"ctrl 9\" },																{ \"style\": \"knob\" }															],															\"init\": 0,															\"min\": 0,															\"max\": 1,															\"step\": 1														}													]												}											]										},										{											\"type\": \"hgroup\",											\"label\": \"Glide and ModMix\",											\"meta\": [												{ \"2\": \"\" }											],											\"items\": [ 												{													\"type\": \"vslider\",													\"label\": \"Glide\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Controllers/Glide and ModMix/Glide\",													\"meta\": [														{ \"0\": \"\" },														{ \"midi\": \"ctrl 5\" },														{ \"scale\": \"log\" },														{ \"style\": \"knob\" },														{ \"tooltip\": \"Portamento (frequency-glide) in seconds per octave\" },														{ \"unit\": \"sec/octave\" }													],													\"init\": 0.008,													\"min\": 0.001,													\"max\": 1,													\"step\": 0.001												},												{													\"type\": \"vslider\",													\"label\": \"Mod. Mix\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Controllers/Glide and ModMix/Mod. Mix\",													\"meta\": [														{ \"1\": \"\" },														{ \"midi\": \"ctrl 48\" },														{ \"style\": \"knob\" },														{ \"tooltip\": \"Modulation Mix: Osc3 (0) to Noise (1)\" }													],													\"init\": 0,													\"min\": 0,													\"max\": 1,													\"step\": 0.001												}											]										}									]								},								{									\"type\": \"vgroup\",									\"label\": \"Oscillator Bank\",									\"meta\": [										{ \"1\": \"\" }									],									\"items\": [ 										{											\"type\": \"hgroup\",											\"label\": \"Oscillator 1\",											\"meta\": [												{ \"1\": \"\" }											],											\"items\": [ 												{													\"type\": \"vslider\",													\"label\": \"Octave1\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Oscillator Bank/Oscillator 1/Octave1\",													\"meta\": [														{ \"1\": \"\" },														{ \"midi\": \"ctrl 23\" },														{ \"style\": \"knob\" }													],													\"init\": 1,													\"min\": 0,													\"max\": 5,													\"step\": 1												},												{													\"type\": \"vslider\",													\"label\": \"DeTuning1\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Oscillator Bank/Oscillator 1/DeTuning1\",													\"meta\": [														{ \"2\": \"\" },														{ \"midi\": \"ctrl 24\" },														{ \"style\": \"knob\" },														{ \"units\": \"Octaves\" }													],													\"init\": 0,													\"min\": -1,													\"max\": 1,													\"step\": 0.001												},												{													\"type\": \"vslider\",													\"label\": \"Waveform1\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Oscillator Bank/Oscillator 1/Waveform1\",													\"meta\": [														{ \"3\": \"\" },														{ \"midi\": \"ctrl 25\" },														{ \"style\": \"knob\" }													],													\"init\": 5,													\"min\": 0,													\"max\": 5,													\"step\": 1												}											]										},										{											\"type\": \"hgroup\",											\"label\": \"Oscillator 2\",											\"meta\": [												{ \"2\": \"\" }											],											\"items\": [ 												{													\"type\": \"vslider\",													\"label\": \"Octave2\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Oscillator Bank/Oscillator 2/Octave2\",													\"meta\": [														{ \"1\": \"\" },														{ \"midi\": \"ctrl 28\" },														{ \"style\": \"knob\" }													],													\"init\": 1,													\"min\": 0,													\"max\": 5,													\"step\": 1												},												{													\"type\": \"vslider\",													\"label\": \"DeTuning2\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Oscillator Bank/Oscillator 2/DeTuning2\",													\"meta\": [														{ \"2\": \"\" },														{ \"midi\": \"ctrl 29\" },														{ \"style\": \"knob\" },														{ \"units\": \"Octaves\" }													],													\"init\": 0.41667,													\"min\": -1,													\"max\": 1,													\"step\": 0.001												},												{													\"type\": \"vslider\",													\"label\": \"Waveform2\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Oscillator Bank/Oscillator 2/Waveform2\",													\"meta\": [														{ \"3\": \"\" },														{ \"midi\": \"ctrl 30\" },														{ \"style\": \"knob\" }													],													\"init\": 5,													\"min\": 0,													\"max\": 5,													\"step\": 1												}											]										},										{											\"type\": \"hgroup\",											\"label\": \"Oscillator 3\",											\"meta\": [												{ \"3\": \"\" }											],											\"items\": [ 												{													\"type\": \"vslider\",													\"label\": \"Octave3\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Oscillator Bank/Oscillator 3/Octave3\",													\"meta\": [														{ \"1\": \"\" },														{ \"midi\": \"ctrl 33\" },														{ \"style\": \"knob\" }													],													\"init\": 0,													\"min\": 0,													\"max\": 5,													\"step\": 1												},												{													\"type\": \"vslider\",													\"label\": \"DeTuning3\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Oscillator Bank/Oscillator 3/DeTuning3\",													\"meta\": [														{ \"2\": \"\" },														{ \"midi\": \"ctrl 34\" },														{ \"style\": \"knob\" },														{ \"units\": \"Octaves\" }													],													\"init\": 0.3,													\"min\": -1,													\"max\": 1,													\"step\": 0.001												},												{													\"type\": \"vslider\",													\"label\": \"Waveform3\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Oscillator Bank/Oscillator 3/Waveform3\",													\"meta\": [														{ \"3\": \"\" },														{ \"midi\": \"ctrl 35\" },														{ \"style\": \"knob\" }													],													\"init\": 0,													\"min\": 0,													\"max\": 5,													\"step\": 1												}											]										}									]								},								{									\"type\": \"vgroup\",									\"label\": \"Mixer\",									\"meta\": [										{ \"2\": \"\" }									],									\"items\": [ 										{											\"type\": \"hgroup\",											\"label\": \"Osc1\",											\"meta\": [												{ \"0\": \"\" }											],											\"items\": [ 												{													\"type\": \"vslider\",													\"label\": \"Osc1 Amp\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Mixer/Osc1/Osc1 Amp\",													\"meta\": [														{ \"0\": \"\" },														{ \"midi\": \"ctrl 26\" },														{ \"style\": \"knob\" }													],													\"init\": 0.5,													\"min\": 0,													\"max\": 1,													\"step\": 0.001												},												{													\"type\": \"vslider\",													\"label\": \"On\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Mixer/Osc1/On\",													\"meta\": [														{ \"1\": \"\" },														{ \"color\": \"blue\" },														{ \"midi\": \"ctrl 12\" },														{ \"style\": \"knob\" }													],													\"init\": 1,													\"min\": 0,													\"max\": 1,													\"step\": 1												}											]										},										{											\"type\": \"hgroup\",											\"label\": \"Ext In, KeyCtl\",											\"meta\": [												{ \"1\": \"\" }											],											\"items\": [ 												{													\"type\": \"vslider\",													\"label\": \"Ext Input\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Mixer/Ext In, KeyCtl/Ext Input\",													\"meta\": [														{ \"0\": \"\" },														{ \"midi\": \"ctrl 27\" },														{ \"style\": \"knob\" }													],													\"init\": 0,													\"min\": 0,													\"max\": 1,													\"step\": 0.001												},												{													\"type\": \"vslider\",													\"label\": \"On\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Mixer/Ext In, KeyCtl/On\",													\"meta\": [														{ \"1\": \"\" },														{ \"color\": \"blue\" },														{ \"midi\": \"ctrl 13\" },														{ \"style\": \"knob\" }													],													\"init\": 0,													\"min\": 0,													\"max\": 1,													\"step\": 1												}											]										},										{											\"type\": \"hgroup\",											\"label\": \"Osc2\",											\"meta\": [												{ \"2\": \"\" }											],											\"items\": [ 												{													\"type\": \"vslider\",													\"label\": \"Osc2 Amp\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Mixer/Osc2/Osc2 Amp\",													\"meta\": [														{ \"0\": \"\" },														{ \"midi\": \"ctrl 31\" },														{ \"style\": \"knob\" }													],													\"init\": 0.5,													\"min\": 0,													\"max\": 1,													\"step\": 0.001												},												{													\"type\": \"vslider\",													\"label\": \"On\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Mixer/Osc2/On\",													\"meta\": [														{ \"1\": \"\" },														{ \"color\": \"blue\" },														{ \"midi\": \"ctrl 14\" },														{ \"style\": \"knob\" }													],													\"init\": 1,													\"min\": 0,													\"max\": 1,													\"step\": 1												}											]										},										{											\"type\": \"hgroup\",											\"label\": \"Noise\",											\"meta\": [												{ \"3\": \"\" }											],											\"items\": [ 												{													\"type\": \"vslider\",													\"label\": \"Noise Amp\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Mixer/Noise/Noise Amp\",													\"meta\": [														{ \"0\": \"\" },														{ \"midi\": \"ctrl 32\" },														{ \"style\": \"knob\" }													],													\"init\": 0,													\"min\": 0,													\"max\": 1,													\"step\": 0.001												},												{													\"type\": \"vgroup\",													\"label\": \"0x00\",													\"meta\": [														{ \"1\": \"\" }													],													\"items\": [ 														{															\"type\": \"vslider\",															\"label\": \"On\",															\"address\": \"/virtualAnalog/Minimoog/0x00/Mixer/Noise/0x00/On\",															\"meta\": [																{ \"0\": \"\" },																{ \"color\": \"blue\" },																{ \"midi\": \"ctrl 15\" },																{ \"style\": \"knob\" }															],															\"init\": 0,															\"min\": 0,															\"max\": 1,															\"step\": 1														},														{															\"type\": \"vslider\",															\"label\": \"White/Pink\",															\"address\": \"/virtualAnalog/Minimoog/0x00/Mixer/Noise/0x00/White/Pink\",															\"meta\": [																{ \"1\": \"\" },																{ \"color\": \"blue\" },																{ \"midi\": \"ctrl 16\" },																{ \"style\": \"knob\" },																{ \"tooltip\": \"Choose either White or Pink Noise\" }															],															\"init\": 1,															\"min\": 0,															\"max\": 1,															\"step\": 1														}													]												}											]										},										{											\"type\": \"hgroup\",											\"label\": \"Osc3\",											\"meta\": [												{ \"4\": \"\" }											],											\"items\": [ 												{													\"type\": \"vslider\",													\"label\": \"Osc3 Amp\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Mixer/Osc3/Osc3 Amp\",													\"meta\": [														{ \"0\": \"\" },														{ \"midi\": \"ctrl 36\" },														{ \"style\": \"knob\" }													],													\"init\": 0.5,													\"min\": 0,													\"max\": 1,													\"step\": 0.001												},												{													\"type\": \"vslider\",													\"label\": \"On\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Mixer/Osc3/On\",													\"meta\": [														{ \"1\": \"\" },														{ \"color\": \"blue\" },														{ \"midi\": \"ctrl 17\" },														{ \"style\": \"knob\" }													],													\"init\": 0,													\"min\": 0,													\"max\": 1,													\"step\": 1												}											]										}									]								},								{									\"type\": \"vgroup\",									\"label\": \"Modifiers\",									\"meta\": [										{ \"3\": \"\" }									],									\"items\": [ 										{											\"type\": \"vgroup\",											\"label\": \"Filter\",											\"meta\": [												{ \"0\": \"\" }											],											\"items\": [ 												{													\"type\": \"hgroup\",													\"label\": \"0x00\",													\"meta\": [														{ \"0\": \"\" },														{ \"tooltip\": \"freq, Q, ContourScale\" }													],													\"items\": [ 														{															\"type\": \"vgroup\",															\"label\": \"0x00\",															\"meta\": [																{ \"0\": \"\" },																{ \"tooltip\": \"two checkboxes\" }															],															\"items\": [ 																{																	\"type\": \"vslider\",																	\"label\": \"Filter Mod.\",																	\"address\": \"/virtualAnalog/Minimoog/0x00/Modifiers/Filter/0x00/0x00/Filter Mod.\",																	\"meta\": [																		{ \"1\": \"\" },																		{ \"color\": \"red\" },																		{ \"midi\": \"ctrl 19\" },																		{ \"style\": \"knob\" },																		{ \"tooltip\": \"Filter Modulation => Route Modulation Mix output to VCF frequency\" }																	],																	\"init\": 1,																	\"min\": 0,																	\"max\": 1,																	\"step\": 1																},																{																	\"type\": \"vslider\",																	\"label\": \"Kbd Ctl\",																	\"address\": \"/virtualAnalog/Minimoog/0x00/Modifiers/Filter/0x00/0x00/Kbd Ctl\",																	\"meta\": [																		{ \"2\": \"\" },																		{ \"midi\": \"ctrl 38\" },																		{ \"style\": \"knob\" },																		{ \"tooltip\": \"Keyboard tracking of VCF corner-frequency (0=none, 1=full)\" }																	],																	\"init\": 1,																	\"min\": 0,																	\"max\": 1,																	\"step\": 0.001																}															]														},														{															\"type\": \"vslider\",															\"label\": \"Corner Freq\",															\"address\": \"/virtualAnalog/Minimoog/0x00/Modifiers/Filter/0x00/Corner Freq\",															\"meta\": [																{ \"1\": \"\" },																{ \"midi\": \"ctrl 74\" },																{ \"style\": \"knob\" },																{ \"tooltip\": \"Corner resonance frequency in Log2(Hertz)\" },																{ \"unit\": \"Log2(Hz)\" }															],															\"init\": 10.6,															\"min\": 5.32193,															\"max\": 14.2877,															\"step\": 1e-06														},														{															\"type\": \"vslider\",															\"label\": \"Corner Resonance\",															\"address\": \"/virtualAnalog/Minimoog/0x00/Modifiers/Filter/0x00/Corner Resonance\",															\"meta\": [																{ \"2\": \"\" },																{ \"midi\": \"ctrl 37\" },																{ \"style\": \"knob\" },																{ \"tooltip\": \"Resonance Q at VCF corner frequency (0 to 1)\" }															],															\"init\": 0.7,															\"min\": 0,															\"max\": 1,															\"step\": 0.01														},														{															\"type\": \"vslider\",															\"label\": \"Amount of Contour (octaves)\",															\"address\": \"/virtualAnalog/Minimoog/0x00/Modifiers/Filter/0x00/Amount of Contour (octaves)\",															\"meta\": [																{ \"3\": \"\" },																{ \"midi\": \"ctrl 39\" },																{ \"style\": \"knob\" }															],															\"init\": 1.2,															\"min\": 0,															\"max\": 4,															\"step\": 0.001														}													]												},												{													\"type\": \"hgroup\",													\"label\": \"Filter Contour\",													\"meta\": [														{ \"1\": \"\" },														{ \"tooltip\": \"AttFilt, DecFilt, Sustain Level for Filter Contour\" }													],													\"items\": [ 														{															\"type\": \"vslider\",															\"label\": \"AttackF\",															\"address\": \"/virtualAnalog/Minimoog/0x00/Modifiers/Filter/Filter Contour/AttackF\",															\"meta\": [																{ \"0\": \"\" },																{ \"midi\": \"ctrl 40\" },																{ \"style\": \"knob\" },																{ \"tooltip\": \"Attack Time\" },																{ \"unit\": \"ms\" }															],															\"init\": 1400,															\"min\": 10,															\"max\": 10000,															\"step\": 1														},														{															\"type\": \"vslider\",															\"label\": \"DecayF\",															\"address\": \"/virtualAnalog/Minimoog/0x00/Modifiers/Filter/Filter Contour/DecayF\",															\"meta\": [																{ \"0\": \"\" },																{ \"midi\": \"ctrl 41\" },																{ \"style\": \"knob\" },																{ \"tooltip\": \"Decay-to-Sustain Time\" },																{ \"unit\": \"ms\" }															],															\"init\": 10,															\"min\": 10,															\"max\": 10000,															\"step\": 1														},														{															\"type\": \"vslider\",															\"label\": \"SustainF\",															\"address\": \"/virtualAnalog/Minimoog/0x00/Modifiers/Filter/Filter Contour/SustainF\",															\"meta\": [																{ \"0\": \"\" },																{ \"midi\": \"ctrl 42\" },																{ \"style\": \"knob\" },																{ \"tooltip\": \"Sustain level as percent of max\" }															],															\"init\": 80,															\"min\": 0,															\"max\": 100,															\"step\": 0.1														}													]												}											]										},										{											\"type\": \"hgroup\",											\"label\": \"Loudness Contour\",											\"meta\": [												{ \"1\": \"\" }											],											\"items\": [ 												{													\"type\": \"vslider\",													\"label\": \"AttackA\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Modifiers/Loudness Contour/AttackA\",													\"meta\": [														{ \"0\": \"\" },														{ \"midi\": \"ctrl 43\" },														{ \"style\": \"knob\" },														{ \"tooltip\": \"Attack Time\" },														{ \"unit\": \"ms\" }													],													\"init\": 2,													\"min\": 0,													\"max\": 5000,													\"step\": 0.1												},												{													\"type\": \"vslider\",													\"label\": \"DecayA\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Modifiers/Loudness Contour/DecayA\",													\"meta\": [														{ \"0\": \"\" },														{ \"midi\": \"ctrl 44\" },														{ \"style\": \"knob\" },														{ \"tooltip\": \"Decay-to-Sustain Time\" },														{ \"unit\": \"ms\" }													],													\"init\": 10,													\"min\": 0,													\"max\": 10000,													\"step\": 0.1												},												{													\"type\": \"vslider\",													\"label\": \"SustainA\",													\"address\": \"/virtualAnalog/Minimoog/0x00/Modifiers/Loudness Contour/SustainA\",													\"meta\": [														{ \"0\": \"\" },														{ \"midi\": \"ctrl 45\" },														{ \"style\": \"knob\" },														{ \"tooltip\": \"Sustain level as percent of max\" }													],													\"init\": 80,													\"min\": 0,													\"max\": 100,													\"step\": 0.1												}											]										}									]								}							]						},						{							\"type\": \"hgroup\",							\"label\": \"Keyboard Group\",							\"meta\": [								{ \"1\": \"\" }							],							\"items\": [ 								{									\"type\": \"vgroup\",									\"label\": \"Wheels and Switches\",									\"meta\": [										{ \"0\": \"\" }									],									\"items\": [ 										{											\"type\": \"hgroup\",											\"label\": \"0x00\",											\"meta\": [												{ \"1\": \"\" },												{ \"tooltip\": \"Wheels+\" }											],											\"items\": [ 												{													\"type\": \"hgroup\",													\"label\": \"0x00\",													\"meta\": [														{ \"1\": \"\" },														{ \"tooltip\": \"Bend and Mod Wheels\" }													],													\"items\": [ 														{															\"type\": \"vslider\",															\"label\": \"Decay\",															\"address\": \"/virtualAnalog/Minimoog/Keyboard Group/Wheels and Switches/0x00/0x00/Decay\",															\"meta\": [																{ \"midi\": \"ctrl 20\" },																{ \"style\": \"knob\" },																{ \"tooltip\": \"Envelope Release either Decay value or 0\" }															],															\"init\": 1,															\"min\": 0,															\"max\": 1,															\"step\": 1														},														{															\"type\": \"vslider\",															\"label\": \"Glide\",															\"address\": \"/virtualAnalog/Minimoog/Keyboard Group/Wheels and Switches/0x00/0x00/Glide\",															\"meta\": [																{ \"midi\": \"ctrl 65\" },																{ \"style\": \"knob\" },																{ \"tooltip\": \"Glide from note to note\" }															],															\"init\": 1,															\"min\": 0,															\"max\": 1,															\"step\": 1														},														{															\"type\": \"hslider\",															\"label\": \"bend\",															\"address\": \"/virtualAnalog/Minimoog/Keyboard Group/Wheels and Switches/0x00/0x00/bend\",															\"meta\": [																{ \"0\": \"\" },																{ \"midi\": \"pitchwheel\" },																{ \"style\": \"knob\" }															],															\"init\": 0,															\"min\": -2,															\"max\": 2,															\"step\": 0.01														},														{															\"type\": \"vslider\",															\"label\": \"mod\",															\"address\": \"/virtualAnalog/Minimoog/Keyboard Group/Wheels and Switches/0x00/0x00/mod\",															\"meta\": [																{ \"1\": \"\" },																{ \"midi\": \"ctrl 1\" },																{ \"style\": \"knob\" },																{ \"tooltip\": \"PitchModulation amplitude in octaves\" }															],															\"init\": 0,															\"min\": 0,															\"max\": 1,															\"step\": 0.01														}													]												}											]										}									]								},								{									\"type\": \"hgroup\",									\"label\": \"0x00\",									\"meta\": [										{ \"1\": \"\" },										{ \"tooltip\": \"Keys\" }									],									\"items\": [ 										{											\"type\": \"hgroup\",											\"label\": \"0x00\",											\"meta\": [												{ \"0\": \"\" },												{ \"tooltip\": \"Gates\" }											],											\"items\": [ 												{													\"type\": \"vslider\",													\"label\": \"gateHold\",													\"address\": \"/virtualAnalog/Minimoog/Keyboard Group/0x00/0x00/gateHold\",													\"meta\": [														{ \"0\": \"\" },														{ \"style\": \"knob\" },														{ \"tooltip\": \"lock sustain pedal on (hold gate set at 1)\" }													],													\"init\": 0,													\"min\": 0,													\"max\": 1,													\"step\": 1												},												{													\"type\": \"button\",													\"label\": \"gate\",													\"address\": \"/virtualAnalog/Minimoog/Keyboard Group/0x00/0x00/gate\",													\"meta\": [														{ \"1\": \"\" },														{ \"tooltip\": \"The gate signal is 1 during a  note and 0 otherwise. For MIDI, NoteOn occurs when the gate  transitions from 0 to 1, and NoteOff is an event corresponding  to the gate transition from 1 to 0. The name of this Faust  button must be 'gate'.\" }													]												},												{													\"type\": \"button\",													\"label\": \"sustain\",													\"address\": \"/virtualAnalog/Minimoog/Keyboard Group/0x00/0x00/sustain\",													\"meta\": [														{ \"1\": \"\" },														{ \"midi\": \"ctrl 64\" },														{ \"tooltip\": \"extends the gate (keeps it set to 1)\" }													]												}											]										}									]								},								{									\"type\": \"hslider\",									\"label\": \"freq\",									\"address\": \"/virtualAnalog/Minimoog/Keyboard Group/freq\",									\"meta\": [										{ \"2\": \"\" },										{ \"style\": \"knob\" },										{ \"unit\": \"Hz\" }									],									\"init\": 220,									\"min\": 0.1,									\"max\": 20000,									\"step\": 0.1								}							]						}					]				}			]		}	]}";
			if (!FaustUI.fJSONParser(ref fJSON, out fUI)) { // Parses the JSON file
      			UnityEngine.Debug.LogError("Error JSON Parser");
			}
			param = fUI.getUI(0).setNumParams(param);   // Sets the parameter number
			_dsp = (FaustPlugin_SAM_VirtualAnalog) target;          // Sets which component will be edited in the inspector
		}

		// @brief Method called when you click on the inspector
		public override void OnInspectorGUI() {
			GUI.enabled = true;
			GUILayout.BeginVertical(); // Each block of elements needs to be wrap by Begin/EndVertical or Begin/EndHorizontal whether it's a vertical or horizontal group
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			// Button to reset the initial parameter value
			if (GUILayout.Button(new GUIContent("Initialization", "Set initial values"), GUILayout.Width(100), GUILayout.Height(20))) {
				for (int i = 0; i < param; i++) {
					changeValueParam(0f, fUI.getUI(0).getItem(i).init, i);
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			// HelpBox to explain how control the parameter
			EditorGUILayout.HelpBox("To control a parameter using scripts, use the parameter number and the setParameter() method", MessageType.Info);
			// Test if the first item is a group (normally it's always true)
			if (fUI.getUI(0).type == "hgroup" || fUI.getUI(0).type == "vgroup" || fUI.getUI(0).type == "tgroup") {
				EditorGUILayout.BeginVertical();
				// the groupstate variable define if the group is opened or closed
				fUI.getUI(0).groupstate = EditorGUILayout.Foldout(fUI.getUI(0).groupstate, fUI.getUI(0).label);
				EditorGUI.indentLevel++;
				if (fUI.getUI(0).groupstate) {
					// if the group is opened, the subgroup are displayed
					for (int i = 0; i < fUI.getUI(0).items.Count; i++) {
						makeLayoutGrp(fUI.getUI(0).items[i]);
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUI.indentLevel--;
			}
			EditorGUI.indentLevel--;
			EditorGUILayout.EndVertical();
		}

		/* @brief Method to display a bargraph
        * @param value Current value of the param in the dsp
        * @param item Which item is displayed */
		private void progressBar(float value, Group item) {
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			if (item.label == "0x00") { // If the name of the parameter hasn't been set, the default name 0x00 is erased
				item.label = "";
			}
			Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
			EditorGUI.ProgressBar(rect, (value - item.min) / (item.max - item.min), item.label); // The range of the toolbar is between 0 an d1 so the values need to be normalize
			Repaint(); // Updates the bargraph value
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}

		/* @brief Method to display a horizontal slider
        * @param value Current value of the param in the dsp
        * @param item Which item is displayed
        * @return the new value of the param */
		private float hSlider(float value, Group item) {
			EditorGUILayout.BeginHorizontal();
			if (item.label == "0x00") { // If the name of the parameter hasn't been set, the default name 0x00 is erased now
				item.label = "";
			}
			float newvalue = EditorGUILayout.Slider(new GUIContent(item.label, helpBox(item)), value, item.min, item.max);
			Repaint();
			EditorGUILayout.EndHorizontal();
			return newvalue;
		}

		/* @brief Method to display a numerical entry
        * @param value Current value of the param in the dsp
        * @param item Which item is displayed
        * @return the new value of the entry */
		private float numEntry(float value, Group item) {
			EditorGUILayout.BeginHorizontal();
			if (item.label == "0x00") { // If the name of the parameter hasn't been set, the default name 0x00 is erased
				item.label = "";
			}
			float newvalue = EditorGUILayout.FloatField(new GUIContent(item.label, helpBox(item)), value);
			Repaint();
			EditorGUILayout.EndHorizontal();
			return newvalue;
		}

		/* @brief Method to display a checkbox
        * @param value Current value of the param in the dsp
        * @param item Which item is displayed
        * @return the new value of the param*/
		private float checkBox(float value, Group item) {
			EditorGUILayout.BeginHorizontal();
			if (item.label == "0x00") { // If the name of the parameter hasn't been set, the default name 0x00 is erased
				item.label = "";
			}
			// A conversion between bool and float needs to be done (Unity: bool; Faust: float)
			bool temp1 = Convert.ToBoolean(value);
			bool temp2 = EditorGUILayout.Toggle(new GUIContent(item.label, helpBox(item)), temp1);
			float newvalue = Convert.ToSingle(temp2);
			Repaint();
			EditorGUILayout.EndHorizontal();
			return newvalue;
		}

		/* @brief Method to display a button
        * @param value Current value of the param in the dsp
        * @param item Which item is displayed
        * @return the new value if the button*/
		private float button(Group item) {
			EditorGUILayout.BeginHorizontal();
			if (item.label == "0x00") { // If the name of the parameter hasn't been set, the default name 0x00 is erased
				item.label = "";
			}
			if (GUILayout.Button(new GUIContent(item.label, helpBox(item)))) {
				Repaint();
				EditorGUILayout.EndHorizontal();
				return 1;
			} else {
				EditorGUILayout.EndHorizontal();
				return 0;
			}
		}

		/* @brief Method to display a helpbox when the mouse is on the parameter, use the metadata from the json
        * @param item Which item is displayed
        * @return the string to display*/
		private string helpBox(Group item) {
			string message = "";
			if (item.meta != null) {
				for (int i = 0; i < item.meta.Count; i++) {
					if (item.meta[i].unit != null) {
						message += "\nUnit : " + item.meta[i].unit;
					}
					if (item.meta[i].scale != null) {
						message += "\nScale : " + item.meta[i].scale;
					}
					if (item.meta[i].tooltip != null) {
						message += "\nDescription : " + item.meta[i].tooltip;
					}
				}
				return message;
			} else {
				return null;
			}
		}

		/* @brief Method to add the different elements on the inspector and deal with parameter variation
        * @param item Which item will be displayed */
		private void addComponent(Group item) {
			int numparam = item.numparam;
			// Tests the type of the item and checks if there is a variation of the parameter
			if (item.type == "vslider" || item.type == "hslider") {
				float value = _dsp.getParameter(numparam);
				float newvalue = hSlider(value, item);
				changeValueParam(value, newvalue, numparam);
			} else if (item.type == "nentry") {
				float value = _dsp.getParameter(numparam);
				float newvalue = numEntry(value, item);
				changeValueParam(value, newvalue, numparam);
			} else if (item.type == "checkbox") {
				float value = _dsp.getParameter(numparam);
				float newvalue = checkBox(value, item);
				changeValueParam(value, newvalue, numparam);
			} else if (item.type == "button") {
				float value = _dsp.getParameter(numparam);
				float newvalue = button(item);
				changeValueParam(value, newvalue, numparam);
			} else if (item.type == "hbargraph" || item.type == "vbargraph") {
				float value = _dsp.getParameter(numparam);
				progressBar(value, item);
			}
		}

		/* @brief Recursive method to organize the layout of the inspector
        * @param item Which item is displayed */
		private void makeLayoutGrp(Group item) {
			// If the items.items is null, the item is actually an item (slider, etc) and not a group because it doesn't contain any item
			if (item.items != null) {
				// If not, the item is a group of items
				if (item.type == "hgroup" || item.type == "vgroup" || item.type == "tgroup") {
					EditorGUILayout.BeginVertical();
					// Display the group
					item.groupstate = EditorGUILayout.Foldout(item.groupstate, item.label);
					// if the group is opened the method checks the items in the group
					if (item.groupstate) {
						EditorGUI.indentLevel++;
						for (int i = 0; i < item.items.Count; i++) {
							makeLayoutGrp(item.items[i]); // Call the same method for each item in the group
						}
						EditorGUI.indentLevel--;
					}
					EditorGUILayout.EndVertical();
				}
			} else {
				addComponent(item); // Add the component to the inspector
			}
		}

		/* @brief Method to change the value of a param in the dsp
        * @param value Value of the param in the dsp
        * @param newvalue Current value of the param in the inspector */
		private void changeValueParam(float value, float newvalue, int param) {
			if (newvalue != value) {
				_dsp.setParameter(param, newvalue);
			}
		}
	}

	#endif // UNITY_EDITOR

	/* @brief This class is the interface between the native plugin and the Unity environment
	*/
	public class Faust_Context {

		private IntPtr _context;

        #if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_WSA_10_0
        const string _dllName = "libFaustPlugin_virtualAnalog";
        #elif UNITY_IOS
        const string _dllName = "__Internal";
        #elif UNITY_EDITOR || UNITY_ANDROID || UNITY_STANDALONE_LINUX
        const string _dllName = "FaustPlugin_virtualAnalog";
        #else
        Debug.LogError("Architecture not supported by the plugin");
        #endif

		// Imports all c++ function to intialize and process the dsp. The methods need to be private
		[DllImport(_dllName)]
		private static extern IntPtr Faust_contextNew(int buffersize);

		[DllImport(_dllName)]
		private static extern void Faust_contextInit(IntPtr ctx, int samplerate);

		[DllImport(_dllName)]
		private static extern void Faust_process(IntPtr ctx, [In] float[] inbuffer, [Out] float[] outbuffer, int numframes, int channels);

		[DllImport(_dllName)]
		private static extern void Faust_delete(IntPtr ctx);

		[DllImport(_dllName)]
		private static extern int Faust_getSampleRate(IntPtr ctx);

		[DllImport(_dllName)]
		private static extern int Faust_getNumInputChannels(IntPtr ctx);

		[DllImport(_dllName)]
		private static extern int Faust_getNumOutputChannels(IntPtr ctx);

		[DllImport(_dllName)]
		private static extern void Faust_setParameterValue(IntPtr ctx, int param, float value);

		[DllImport(_dllName)]
		private static extern float Faust_getParameterValue(IntPtr ctx, int param);

		[DllImport(_dllName)]
		private static extern float Faust_getParamMin(IntPtr ctx, int param);

		[DllImport(_dllName)]
		private static extern float Faust_getParamMax(IntPtr ctx, int param);

		public Faust_Context(int buffersize) {
			_context = Faust_contextNew(buffersize);
		}

		~Faust_Context() {
			Faust_delete(_context);
		}

		public void context_init(int samplerate) {
			Faust_contextInit(_context, samplerate);
		}

		public int getSampleRate() {
			return Faust_getSampleRate(_context);
		}

		public int getNumInputChannels() {
			return Faust_getNumInputChannels(_context);
		}

		public int getNumOutputChannels() {
			return Faust_getNumOutputChannels(_context);
		}

		public void process(float[] buffer, int numframes, int channels) {
			Faust_process(_context, buffer, buffer, numframes, channels);
		}

		public void setParameterValue(int param, float value) {
			Faust_setParameterValue(_context, param, value);
		}

		public float getParameterValue(int param) {
			return Faust_getParameterValue(_context, param);
		}

		public float getParamMin(int param) {
			return Faust_getParamMin(_context, param);
		}

		public float getParamMax(int param) {
			return Faust_getParamMax(_context, param);
		}

	}

	/* @brief Class to parse the JSON
	* @brief These attributes represent the global data except the global metadata (useless in this context)
	* @brief Also contains all methods to parse the JSON
	*/
	public class FaustUI {
		public string name;
		public int inputs;
		public int outputs;
		public List < Group > ui;

		public Group getUI(int i) {
			return ui[i];
		}

		public static bool fJSONParser(ref string fJSON, out FaustUI faustUI) {
			faustUI = new FaustUI();
			bool success = false;
			StringBuilder key,
			value;
			if (parseChar(ref fJSON, '{')) {
				do {
					if (parseDQString(ref fJSON, out key) && parseChar(ref fJSON, ':')) {
                        switch (key.ToString()) {
						case "name":
							success = parseDQString(ref fJSON, out value);
							faustUI.name = value.ToString();
							break;
                        case "filename":            // Don't use the result
                            success = parseDQString(ref fJSON, out value);
                            break;
                        case "version":             // Don't use the result
                            success = parseDQString(ref fJSON, out value);
                            break;
                        case "compile_options":     // Don't use the result
                            success = parseDQString(ref fJSON, out value);
                            break;
                        case "library_list":        // Don't use the result
                            success = parseGlobalMetaData(ref fJSON);
                            break;
                        case "include_pathnames":   // Don't use the result
                            success = parseGlobalMetaData(ref fJSON);
                            break;
						case "inputs":
							success = parseNum(ref fJSON,  out value);
							faustUI.inputs = Convert.ToInt32(value.ToString());
							break;
						case "outputs":
							success = parseNum(ref fJSON,  out value);
							faustUI.outputs = Convert.ToInt32(value.ToString());
							break;
						case "meta":
							success = parseGlobalMetaData(ref fJSON);
							break;
	                    case "ui":
							faustUI.ui = new List < Group > ();
							int numitems = 0;
							success = parseUI(ref fJSON, ref faustUI.ui, ref numitems);
							break;
						default:
                            // Unknown items should be parsed
                            success = parseDQString(ref fJSON, out value);
							break;
						}
					}
				} while (parseChar(ref fJSON, ','));
			}
			parseChar(ref fJSON, '}');
			return success;
		}

		private static void skipBlank(ref string s) {
			while (Char.IsWhiteSpace(s.ToCharArray()[0])) {
				s = skipChar(ref s, 1);
			}
		}

		private static bool parseChar(ref string s, char x) {
			skipBlank(ref s);
			if (s.ToCharArray()[0] == x) {
				s = skipChar(ref s, 1);
				return true;
			} else {
				return false;
			}
		}

		private static string skipChar(ref string s, int count) {
			return s.Remove(0, count);
		}

		private static bool parseString(ref string s, char quote, out StringBuilder name) {
			bool valid = false;
			name = new StringBuilder();
			skipBlank(ref s);
			char[] c = s.ToCharArray();
			if (c[0] == quote) {
				int i = 1;
				while (c[i] != quote) {
					name.Append(c[i]);
					i++;
				}
				valid = true;
				s = skipChar(ref s, name.ToString().Length + 2);
			}
			return valid;
		}

		private static bool parseNum(ref string s,  out StringBuilder name) {
			bool valid = false;
			name = new StringBuilder();
			char[] c = s.ToCharArray();
			if (c[0] == ' ') {
				int i = 1;
				while (c[i] != ' ' && c[i] != ',' && c[i] != '}' && c[i] != ']') {
					name.Append(c[i]);
					i++;
				}
				valid = true;
				s = skipChar(ref s, name.ToString().Length + 1);
			}
			return valid;
		}

		private static bool parseDQString(ref string s, out StringBuilder word) {
			return parseString(ref s, '"', out word);
		}

		private static bool parseItemMetaData(ref string s, List < Meta > metadatas) {
			StringBuilder metaKey,
			metaValue;
			if (parseChar(ref s, ':') && parseChar(ref s, '[')) {
				do {
					if (parseChar(ref s, '{')
                        && parseDQString(ref s, out metaKey)
                        && parseChar(ref s, ':')
                        && parseDQString(ref s, out metaValue)
                        && parseChar(ref s, '}')) {
						switch (metaKey.ToString()) {
						case "unit":
							Meta meta1 = new Meta();
							meta1.unit = metaValue.ToString();
							metadatas.Add(meta1);
							break;
						case "tooltip":
							Meta meta2 = new Meta();
							meta2.tooltip = metaValue.ToString();
							metadatas.Add(meta2);
							break;
						case "scale":
							Meta meta3 = new Meta();
							meta3.scale = metaValue.ToString();
							metadatas.Add(meta3);
							break;
						default:
							break;
						}
					}
				} while (parseChar(ref s , ','));
				return parseChar(ref s, ']');
			} else {
				return false;
			}
		}

		private static bool parseGlobalMetaData(ref string s) {
			if (parseChar(ref s, '[')) {
				while (!parseChar(ref s, ']')) {
					s = skipChar(ref s, 1);
				}
				return true;
			} else {
				return false;
			}
		}

		private static bool parseUI(ref string s, ref List < Group > uiItems, ref int numItems) {
			CultureInfo culture = new CultureInfo("en-US");
			if (parseChar(ref s, '[')) {

				StringBuilder label;
				StringBuilder value;

				do {
					if (parseChar(ref s, '{')) {
						do {
							if (parseDQString(ref s, out label)) {

								switch (label.ToString()) {
								case "type":
									if (uiItems.Count != 0) {
										numItems++;
									}
									if (parseChar(ref s, ':') && parseDQString(ref s, out value)) {
										Group item = new Group();
										item.type = value.ToString();
										uiItems.Add(item);
									}
									break;
								case "label":
									if (parseChar(ref s, ':') && parseDQString(ref s, out value)) {
										uiItems[numItems].label = value.ToString();
									}
									break;
								case "address":
									if (parseChar(ref s, ':') && parseDQString(ref s, out value)) {
										uiItems[numItems].address = value.ToString();
									}
									break;
								case "meta":
									uiItems[numItems].meta = new List < Meta > ();
									if (!parseItemMetaData(ref s, uiItems[numItems].meta)) {
										return false;
									}
									break;
								case "init":
									if (parseChar(ref s, ':') && parseNum(ref s, out value)) {
										float x = Convert.ToSingle(value.ToString(), culture);
										uiItems[numItems].init = x;
									}
									break;
								case "min":
									if (parseChar(ref s, ':') && parseNum(ref s, out value)) {
										float x = Convert.ToSingle(value.ToString(), culture);
										uiItems[numItems].min = x;
									}
									break;
								case "max":
									if (parseChar(ref s, ':') && parseNum(ref s, out value)) {
										float x = Convert.ToSingle(value.ToString(), culture);
										uiItems[numItems].max = x;
									}
									break;
								case "step":
									if (parseChar(ref s, ':') && parseNum(ref s, out value)) {
										float x = Convert.ToSingle(value.ToString(), culture);
										uiItems[numItems].step = x;
									}
									break;
								case "items":
									uiItems[numItems].items = new List < Group > ();
									int numItems2 = 0;
									if (parseChar(ref s, ':') && !parseUI(ref s, ref uiItems[numItems].items, ref numItems2)) return false;
									break;
								default:
									return false;
								}
							} else {
								return false;
							}
						} while (parseChar(ref s , ','));
						parseChar(ref s, '}');
					} else {
						return false;
					}
				} while (parseChar(ref s , ','));
				return parseChar(ref s, ']');
			} else {
				return false;
			}
		}
	}

	/* @brief Class representing an item (slider, etc) or a group
    * @brief if it's an item List<Group> is null */
	public class Group {
		public string type;
		public string label;
		public List < Meta > meta;
		public List < Group > items;
		public string address;
		public float init;
		public float min;
		public float max;
		public float step;
		public int numparam;
		public bool groupstate = true;

		/* @brief Recursive method to set the number of a parameter
        * @brief it gives a unique ID to the parameters and uses it to communicate with the plugin
        */
		public int setNumParams(int param) {
			if (items != null) {
				for (int i = 0; i < items.Count; i++) {
					param = items[i].setNumParams(param);
				}
				return param;
			} else {
				numparam = param;
				param++;
				return param;
			}
		}

		public Group getItem(int param) {
			Group result = null;
			if (items != null) {
				for (int i = 0; i < items.Count; i++) {
					Group item = items[i].getItem(param);
					if (item != null) {
						result = item;
					}
				}
				return result;
			} else {
				if (this.numparam == param) {
					return this;
				} else {
					return null;
				}
			}
		}
	}

	/* @brief Class to parse the metadata of an item (not the global metadata)
	*/
	public class Meta {
		public string unit;
		public string scale;
		public string tooltip;
	}
}
