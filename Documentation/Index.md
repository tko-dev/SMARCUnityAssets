- [SMaRC Unity Assets](#smarc-unity-assets)
  - [Force](#force)
  - [Water](#water)
  - [Vehicle Components](#vehicle-components)
      - [LinkAttachment](#linkattachment)
    - [Acoustics](#acoustics)
      - [Transceiver](#transceiver)
    - [Actuators](#actuators)
      - [Hinge](#hinge)
      - [Prismatic](#prismatic)
      - [Propeller](#propeller)
      - [VBS](#vbs)
    - [Sensors](#sensors)
      - [Sensor](#sensor)
      - [Battery](#battery)
      - [Camera Image](#camera-image)
      - [DepthPressure](#depthpressure)
      - [DVL](#dvl)
      - [GPS](#gps)
        - [GPSReferencePoint](#gpsreferencepoint)
      - [IMU](#imu)
      - [Leak](#leak)
      - [Sonar](#sonar)
    - [ROS](#ros)
      - [Core](#core)
        - [RosMessages](#rosmessages)
        - [Clock](#clock)
        - [ROSPublisher](#rospublisher)
      - [Publishers](#publishers)
      - [Subscribers](#subscribers)
  - [Rope](#rope)
  - [Importer](#importer)
  - [Drone](#drone)
  - [GameUI](#gameui)

# SMaRC Unity Assets

## Force
Simulates a point mass interacting with the water surface.

## Water
Simulates currents within a volume, uses [Force](#force) points.

## Vehicle Components
These are the components that can be compiled within a vehicle. Most of them reflect real objects and some are abstractions.

#### LinkAttachment
A base class that most sensors and actuators derive from. This enables separation of structure from function. 

### Acoustics
Peer to peer acoustic communication items.

#### Transceiver
Simulates both a receiver and a transmitter of acoustic signals at the package level. 
Features:
- Surface echoes
- Bottom echoes
- Occlusions
- Travel-time

### Actuators
Scripts that control articulation bodies using drives defined within.

#### Hinge
A simple hinge. Rotates.

#### Prismatic
A piston. Moves back and forth.

#### Propeller
A propeller blade. Turns, pushes and torques.

#### VBS
A tank. Fills up and empties to change its mass.

### Sensors
They sense things and store the information within.

#### Sensor
The base class of most sensors. Handles time related stuff in a general way.

#### Battery
A battery. Discharges over time.

#### Camera Image
A camera. Attach a Unity camera too.

#### DepthPressure
Outputs depth through pressure. Uses [water](#water).

#### DVL
Beams into velocities.

#### GPS
Globally positions the system. 

##### GPSReferencePoint
Somewhere in Unity is somewhere in real world.

#### IMU
Internally measures units.

#### Leak
Not the vegetable.

#### Sonar
Ping.


### ROS

#### Core

##### RosMessages
Where all the messages live.

##### Clock
Tick tock

##### ROSPublisher
Base class of most publishers. Handles timing stuff.

#### Publishers
`_Pub`

#### Subscribers
`_Sub`


## Rope
Lots of strings attached.

## Importer
URDF into Unity, JSON out of Unity

## Drone
It flies.

## GameUI
Looks pretty? Sometimes.
