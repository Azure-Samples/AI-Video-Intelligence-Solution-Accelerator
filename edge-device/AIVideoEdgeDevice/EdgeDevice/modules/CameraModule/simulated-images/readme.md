Images in this directory have been altered from RBG to BRG to accommodate the Machine Learning process

The 'port' property of a camera definition within CameraModule's Module Twin causes the following
behaviors for simulated cameras. Do not include the file extension in the port name. Files must
exist within the CameraModule's `simulated-images` directoy as jpeg and within the
`simulated-images/300x300` as a 300x300 png.

* A specific filename such as `counter` or `cycle-0-2` -- always displays that particular file
* The name of a cycle such as 'cycle-0' -- cycles through the images which start with that cycle name
