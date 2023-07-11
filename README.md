# SLC-AS-SetBaseline
An Automation script to set the baseline of a parameter

## Input parameters
* **DVE Alarm Template On Main Element**: If you are setting the baseline of a DVE parameter, and the alarm template is set on the main element, set this value to 'True', otherwise to 'False'.
* **Element Name**: The name of the element you are changing the baseline of.
*  **Max**: This can be used to make sure the value that is set is not exeeding a specific maximum value, can be used together witht he 'Current Value' feature (see the **Value** input Parameter). If no max value is required, can be set to 'NA'.
*  **Min**: See Max.
*  **Parameter ID**: The (read) parameter ID if the parameter that you are changing the baseline for.
*  **Value**: The specific value you want to set as the baseline, or 'Current Value' if you want to set the current value as the baseline.

## Visio usage
```
Script:Set Baseline||Element Name=[this Element];Parameter ID=100;DVE Alarm Template On Main Element=False;Value=Current Value;min=0;max=99||Set the Alarm baseline to the current value|NoConfirmation,NoSetCheck,CloseWhenFinished
```