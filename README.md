# ev3dev_csharp

C# Library for Lego Mindstorms EV3 (running on ev3dev). Most code has inline documentation, so if you use Visual Studio or Visual Studio Code you will be provided with quite detailed description of classes, methods and enum constants. Here is a short list of project contents.

You need to install Mono (5 or higher) on your Ev3Dev-supported device to use this.

## Ev3Dev.CSharp assembly

### Ev3Dev API implementation
Introduces partial (yet useful) [Ev3Dev API](https://ev3dev-lang.readthedocs.io/en/latest/classes.html) implementation. This covers:
- **Device** - base Ev3Dev device abstraction
- **Motor** - a uniform interface for using motors with positional and directional feedback such as the EV3 and NXT motors
  - Includes string constants for large and medium EV3 motor drivers
- **Sensor** - a uniform interface for using most of the sensors available for the EV3

### Sound API

C# interface for Linux sound utilities: `beep`, `aplay`, and `espeak`.

### LazyTask

Utility class for most other high-level interfaces - derivative of System.Threading.Task that will call its internal action only if someone tries to await it. This is intended for device state waiting operations which include device polling, which is quite expensive operation especially if you do not want it to be performed at background.

## Ev3Dev.CSharp.BasicDevices assembly

This assembly contains high-level API for all stock Lego Mindstorms EV3 devices (this means that you will work with them in terms of methods and language features, not in terms of string and int properties). Such as:

```C#
using (var motor = new LargeMotor(OutputPort.OutB))
using (var colorSensor = new ColorSensor(InputPort.In2))
{
    colorSensor.Mode = ColorSensorMode.Color;
    await motor.Run(rotations: 1.0f, power: 75);
    await motor.RunTimed(ms: 1000, power: 75);
    Console.WriteLine($"I see {colorSensor.Color} color");
}
```

The devices APIs are designed with my own vision of handiness and sanity, but I hope you will find it useful too.

## Ev3Dev.CSharp.EvA

Experimental **Ev**ent driven **A**rchitecture framework for building your robots that can react on surroundings, not just execute a list of commands step by step. This is intended for building event loop-based robots in a declarative style, highly inspired by ASP.NET MVC framework. With this, you can define your robot just as

```C#
class MyRobot
{
    private ColorSensor _colorSensor;
    private LargeMotor _motor;
    
    public bool IsDark => _colorSensor.LightIntensity < 2;
    
    [ShutdownEvent]
    public bool IsScared { get; set; }
    
    [EventHandler("IsDark")]
    public void GetScared()
    {
        IsScared = true;
        Sound.Speak("It's dark, I'm scared", wordsPerMinute: 130, amplitude: 300);
    }
    
    [Action]
    public void DriveForward()
    {
        _motor.Run(rotations: 1.0f);
    }
}
```

This supports async methods, flexible exception handling, navigating values from properties to action arguments (to sustain values consistency during the iteration), and plenty of other features. There is also a plugin API (in fact, `Action` and `EventHandler` attributes are implemented as plugins). There are many tests to be written and even more APIs to be documented, so stay tuned.

## Ev3Dev.CSharp.Demos assembly

A handful of samples for you to get familiar with APIs and capabilities.
