# _looper_

![](/img/looper.jpeg)

looper is a depth video delay that facilitates an out-of-body third-person view of yourself. participants can see their own image as captured from a kinect sensor, from inside a virtual reality headset. Delayed copies of the depth video are then overlaid on top of the original source, allowing participants to see their past selves from a third-person point of view. older selves are progressively downsampled until they disintegrate into nothingness.

## Installation

Download the latest build from the [releases](https://github.com/yariza/looper/releases) page.

- [Oculus Rift Setup](https://www.oculus.com/setup/)
- [Kinect for Windows Runtime 2.0](https://www.microsoft.com/en-us/download/details.aspx?id=44559)

## Setup and Calibration

The experience requires an Oculus Rift, Kinect v2 sensor, and Kinect for Windows adapter.

Setup the Oculus Rift tracking space. Point the Kinect, such that the tracking space is fully visible from the camera.

To run the experience, the position of the Kinect must be calibrated to line up the point cloud with reality. On the number pad, these controls are (local to the Kinect):

- Keypad 4 - Move left
- Keypad 6 - Move right
- Keypad 8 - Move forward
- Keypad 5 - Move backward
- Keypad - - Move down
- Keypad + - Move up
- Keypad 7 - Yaw left
- Keypad 9 - Yaw right
- Keypad 1 - Pitch down
- Keypad 3 - Pitch up

Additionally, there is a bounding box that culls pixels that are outside of the bounds. Press B to toggle the bounding box visibility, then adjust the position and scale of the box to fit the confines of what you want to make visible:

- J - Move left
- L - Move right
- I - Move forward
- K - Move backward
- U - Move down
- O - Move up
- ; - Scale X down
- ' - Scale X up
- [ - Scale Y down
- ] - Scale Y up
- \- \- Scale Z down
- \+ \- Scale Z up
- 9 - Yaw left
- 0 - Yaw right

Once the experience is calibrated, the settings will be saved across sessions.

## Debug GUI / Hotkeys

- C - Toggle third-person camera
- V - Toggle camera auto-orbit
- WASD Left Shift Space - Move camera (only when auto-orbit disabled)
- Arrow Keys - Rotate camera (only when auto-orbit disabled)
- H - Toggle FPS counter
