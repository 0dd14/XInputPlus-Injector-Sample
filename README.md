# XInputPlus-Injector-Sample

This is a sample program to inject XInputplus (https://0dd14lab.net/bin/xinputplus) into a running process.

## Description
InjectorDLL "XInputPlusInjector.dll" included with XInputPlus has an internal implementation of inject function into the running process.  

Follow steps below to use it. 
  
1. Write the location paths of injector DLLs, XInputPlus DLLs, and other settings in a memory maped file.  
2. Call "HookProcss" function of injectorDLL.
   
## Usage this sample
When running this program, you will need to place the files as follows

1. copy  ***"x86"*** and ***"x64"*** folders of XInputPlus package in same folder as this program. 
2. copy ***"XInputPlusInjector.dll"*** and ***"XInputPlusInjector64.dll"*** in "loader" folder of the XInputPlus package in same folder as this program.
3. Place XInputPlus configuration file ***"XInputPlus.ini"*** in same folder as this program.

the folder structure is as follows.
~~~
+ XIPlusInjector.exe (this sample)
+ XInputPlus.ini
+ XInputPlusInjector.dll
+ XInputPlusInjector64.dll 
+ x86
  + DInput.dl_  
  + DInput8.dl_  
  + XInput1_3.dl_   
+ x64
  + DInput.dl_  
  + DInput8.dl_  
  + XInput1_3.dl_
~~~

### Execute program
Execute on following command line
~~~
XIPlusInjector [TargetProcessID]
~~~

## Note
+ Successful injection of XInputplus depends on the target program. Not all programs will be injected successfully.
+ This function of XInputplusInjector is for internal processing. This functionality is subject to change with XInputPlus version upgrades.
