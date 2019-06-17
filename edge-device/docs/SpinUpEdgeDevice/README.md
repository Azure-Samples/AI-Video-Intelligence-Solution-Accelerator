# Spinning Up an AI VIdeo Edge Device

Prerequisites:
* Azure Subscription
* IoT Hub
* Container Registry (jetcontainerregistry) with 3 Repositories:
  * CameraModule
  * Classifer
  * VideoProcessorModule
* Storage Account with a blob called still-images
* A Unix machine or VM

This article describes the manual process of spinning up a new Azure IoT Edge device on an Ubuntu Linux x64 machine. Hopefully this process will become simpler once we have deployments configured.

## Register a New Device with the IoT Edge Hub
1. In the Azure Portal open the the IoT Hub that will host your device

2. Under **Automatic Device Management**, select **IoT Edge**

3. Near the top of the page, select **Add an IoT Edge device**

4. Provide a name in the **Device ID** section, then click **Save**. This will create a new device registration in your IoT Hub.

5. Click on the new device in the device list and copy the device connection string from the device details page. Save this for later use.

## Populate the Device with the Required Modules
The device modules are stored in a container registry. In our case it is the JetContainerRegistry, in the AIVideoDevices resource group.

Before you can add your first module you will need access to information about the container registry. I recommend opening a separate portal tab to make it easier to transfer information from this page.
1. Open the container registry in a new browser tab.

2. Select **Access keys**. This page contains the information you will need to add modules to the device.

3. Return to the device details page. In the upper-left portion of the device details page, select **Set modules**. The upper portion of the set modules page contains four text areas which must be filled in with information about the container registry.

    * From the container registry access keys page, copy the **Registry name** into the **NAME** text area.
    * Copy the **Login server** into the **ADDRESS** text area
    * Copy the **Username** into the **USER NAME** text area
    * Copy one of the passwords into the **PASSWORD** area

4. Return to the container registry tab and select **Repositories**. This will provide a list of container repositories, which you'll refer to shortly.

Now we're ready to add our modules. We'll start by adding the camera module.
1. Returning to the device details, in the lower section of the Set modules page, under **Deployment Modules**, click on **Add**.

2. From the drop-down menu select **IoT Edge Module**. You will be asked for the name and the address of the module being added.

3. Name the module "CameraModule".  Note that case matters on the module names.

4. The module URI is a combination of the registry login server and the identifier for the container, for example `jetcontainerregistry.azurecr.io/cameramodule:0.0.7-amd64`. To find the container identifier:
    * On the container repository **Repositories** page, select **cameramodule**. From the list of containers select the newest version, which should be at the top of the list.
    * The identifier of the module container is displayed at the top of the page, e.g., `cameramodule:0.0.7-amd64`.

5. For the camera module, the module twin must be configured. Click the checkbox for **Set module twin's desired properties**. Each device will have different configuration values for the camera module. Here is an example module twin for a camera module configured for two cameras:
```json
{
  "properties.desired": {
    "cameras": {
      "cam01": {
        "port": "USB\\VID_045E&PID_07A5",
        "id": "bldg52/room302/grid01x04look42",
        "type": "usb",
        "simulated": true
      },
      "cam02": {
        "port": "USB\\VID_045E&PID_07A6",
        "id": "bldg52/room304/grid01x04look44",
        "type": "usb",
        "simulated": true
      }
    }
  }
}
```

6. Click **Save**

7. Click **Add** to add another module. This time we'll add the video processor module, naming it "VideoProcessorModule". Provide the URI of the module as described in step 4 above.

8. Generate a SAS Url for your container :
    * Go to the storage account
    * Under Settings, Select the Shared Access Signature
    * On **Allowed services** select Blob only
    * On **Allowed resource types** select Container + Object
    * On **Allowed permissions** select Write + Create
    * Leave Start time as-is
    * Select some appropriate End time
    * Click the **Generate SAS and connection string** button, use the generated connection string for the contents of blobStorageSasUrl in the next step

9. Modify the module twin. The module twin should look like
```json
{
  "properties.desired": {
    "blobStorageSasUrl": "BlobEndpoint=https://storage52wtl.blob.core.windows.net/;QueueEndpoint=https://storage52wtl.queue.core.windows.net/;FileEndpoint=https://storage52wtl.file.core.windows.net/;TableEndpoint=https://storage52wtl.table.core.windows.net/;SharedAccessSignature=sv=2018-03-28&ss=b&srt=co&sp=wc&se=2020-04-05T05:00:34Z&st=2019-04-20T21:00:34Z&spr=https&sig=BAjwrvjBRMSN8iRRrcB6g5B0zvh4MNsmLF%2BpE1rBeE8%3D",
    "recognitionThreshold": 0.999999
  }
}
```



10. Click **Save**.

11. Click **Add** to add a third module. This will be the classifier ML module which we'll simply name "classifier". There is no module twin data for this module, so leave the checkbox unchecked.  Click **Save**

12. Click **Next** to advance to page 2, **Specify Routes**. There are no required changes here, so click **Next** to advance to page 3, **Review Deployment**. Click **Submit**.

## Configure the Device
To be an IoT Edge device, the device must be running the IoT Edge runtime, which must be installed onto the device. For detailed information about installing the software, refer to [https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-linux](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-linux). Following is the basic procedure. These steps are performed on the Linux device, at the command-line.

NOTE sometimes SSH-ing into the Azure VMs created via the CLI fails: "Disconnected: no supported authentication methods available"  If you get this error, find the VM through the azure portal and change the password for the login account.  

### Register Microsoft key and software repository feed
Install the repository configuration.
> Choose the code snippet for the Ubuntu version you're using. Most new versions of Ubuntu are 18.04.

For Ubuntu **16.04**
```bash
curl https://packages.microsoft.com/config/ubuntu/16.04/prod.list > ./microsoft-prod.list
```

For Ubuntu **18.04**
```bash
curl https://packages.microsoft.com/config/ubuntu/18.04/prod.list > ./microsoft-prod.list
```

Copy the generated list
```bash
sudo cp ./microsoft-prod.list /etc/apt/sources.list.d/
```

Install the Microsoft GPG public key
```bash
curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
sudo cp ./microsoft.gpg /etc/apt/trusted.gpg.d/
```

### Install the container runtime
Perform apt update
```bash
sudo apt-get update
```

Install the Moby engine
```bash
sudo apt-get install moby-engine
```

Install the Moby command-line interface
```bash
sudo apt-get install moby-cli
```

### Install the Azure IoT Edge Security Daemon
Perform apt update
```bash
sudo apt-get update
```

Install the security daemon. The package is installed at `/etc/iotedge/`
```bash
sudo apt-get install iotedge
```

### Configure the Azure IoT Edge Security Daemon
To associate this device with the device we registered with the IoT Hub in the portal, we configure the device with a device connection string.

1. Edit the configuration file.
```bash
sudo nano /etc/iotedge/config.yaml
```

```nano``` is a simple text-mode text editor. Locate the lines in the configuration file that look like 

```yaml
provisioning:
  source: "manual"
  device_connection_string: "<ADD DEVICE CONNECTION STRING HERE>"
```

2. Copy the connection string over the placeholder, keeping the quotation marks. If you did not save the connection string earlier, you can retrieve it from the device details page in the portal.

3. Save the file and exit the editor:

> `Ctrl+X`, `Y`, `ENTER`

4. Restart the daemon.
```bash
sudo systemctl restart iotedge
```

### Verify the installation
Check the status of the daemon, you should see **Active status (running)**
```bash
systemctl status iotedge
```

Check that the expected modules, (video processor, classifier, and camera) are present.  It make take a few minutes after configuring the device for the modules to appear in the list.
```bash
sudo iotedge list
```


To see the output fro the video module:
```bash
sudo iotedge logs VideoProcessorModule -f --tail 20
```

### For Troubleshooting 

To stop IoTEdge with the command
```bash
sudo systemctl stop iotedge
```

To start IoTEdge with the command
```bash
sudo systemctl start iotedge
```