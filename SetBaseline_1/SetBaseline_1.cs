/*
****************************************************************************
*  Copyright (c) 2023,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

dd/mm/2023	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.Messages;

public class Script
{
	public void Run(Engine engine)
	{
		// Get element
		ScriptParam elementNameParam = engine.GetScriptParam("Element Name");
		Element element = engine.FindElement(elementNameParam.Value);

		// Get DVE Template On Main Element
		ScriptParam DVEAlarmTemplateOnMainElementParam = engine.GetScriptParam("DVE Alarm Template On Main Element");
		bool DVEAlarmTemplateOnMainElement = DVEAlarmTemplateOnMainElementParam.Value == "True" ? true : false; // Expecting "True" or "False"

		// Get parameter ID
		ScriptParam paramIdParam = engine.GetScriptParam("Parameter ID");
		int pid = Convert.ToInt32(paramIdParam.Value);

		// Get Value
		// If this value isn't empty, it will be used as baseline value instead of the current value
		ScriptParam valueParam = engine.GetScriptParam("Value");
		string sValue = valueParam.Value;

		if (string.IsNullOrEmpty(sValue) || sValue.ToLower() == "current value") // No Value was defined, get the current value of the parameter
		{
			// Get Parameter Value
			object value = element.GetParameter(pid);
			if (value == null)
			{
				engine.GenerateInformation(string.Format("Abort script : value for parameter {0} is null and no value was passed into the automation script.", pid));
				return;
			}

			sValue = value.ToString();
		}

		// if it is a numeric value, check the limits
		if (double.TryParse(sValue, out double dValue))
		{
			// Get Limits
			string sMax = engine.GetScriptParam("Max").Value;
			double max = double.MaxValue;
			if (sMax != "NA")
			{
				max = Convert.ToDouble(sMax, CultureInfo.InvariantCulture);
			}

			string sMin = engine.GetScriptParam("Min").Value;
			double min = double.MinValue;
			if (sMin != "NA")
			{
				min = Convert.ToDouble(sMin, CultureInfo.InvariantCulture);
			}

			// Do the check
			if (!(min < dValue && max > dValue))
			{
				engine.GenerateInformation(string.Format("The value({0}) can't be set, it's outside of the limits (min: {1}, max: {2}).", dValue, min, max));
				return;
			}
		}

		// Set Baseline
		// If we are setting the baseline of a DVE parameter, and the alarm template is set on the main element, we have to set the baseline on the main element.
		// if the DVE has it's alarm template on the DVE itself, we need to set the baseline on the DVE.
		if (DVEAlarmTemplateOnMainElement)
		{
			Element mainElement = FindMainElement(engine, element);
			SetBaseline(engine, mainElement, pid, sValue, elementNameParam.Value);
		}
		else
		{
			SetBaseline(engine, element, pid, sValue);
		}
	}

	public static Element FindMainElement(Engine engine, Element DVE)
	{
		GetLiteElementInfo liteElementInfo = new GetLiteElementInfo();
		liteElementInfo.DataMinerID = DVE.DmaId;
		liteElementInfo.ElementID = DVE.ElementId;

		LiteElementInfoEvent responseMessage = engine.SendSLNetSingleResponseMessage(liteElementInfo) as LiteElementInfoEvent;

		if (responseMessage.IsDynamicElement)
		{
			return engine.FindElement(responseMessage.DveParentDmaId, responseMessage.DveParentElementId);
		}
		else
		{
			throw new Exception("Error: Invalid request to find Main element of non-DVE");
		}

	}

	public static void SetBaseline(Engine engine, Element element, int pid, string value, string rowKey = null)
	{
		Skyline.DataMiner.Net.Messages.SetNormalizedParameterValuesMessage message = new SetNormalizedParameterValuesMessage(element.DmaId, element.ElementId);
		if (string.IsNullOrEmpty(rowKey))
		{
			engine.GenerateInformation(string.Format("Setting baseline for element {0}, parameter {1} to {2} by {3}", element.ElementName, pid, value, engine.UserDisplayName));
			message.NormalizedValues = new ParameterNormalizedValue[]
			{
				new ParameterNormalizedValue(pid, value)
			};
		}
		else
		{
			engine.GenerateInformation(string.Format("Setting baseline for element {0}, parameter {1}, key {2} to {3} by {4}", element.ElementName, pid, rowKey, value, engine.UserDisplayName));
			message.NormalizedValues = new ParameterNormalizedValue[]
			{
				new ParameterNormalizedValue(pid, value, rowKey)
			};
		}

		var responseMessage = engine.SendSLNetSingleResponseMessage(message) as GetNormalizedParameterValuesResponseMessage;
	}
}