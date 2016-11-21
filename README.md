# AndroidSlideShowCapture
take photos of slideshow images on android phone remotely using windows forms application and ADB

Application starts ADB(https://developer.android.com/studio/command-line/adb.html) connection with first connected android device and turns on the screen, opens OpenCamera app(http://opencamera.sourceforge.net/) that has to be preinstalled on your device and then starts full screen slideshow on secondary screen if one is connected or on primary screen otherwise.

On every slideshow image transition android OpenCamera is triggered and image is saved to device. When all slideshow images have been shown and captured, application closes OpenCamera app and exits!
