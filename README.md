# SlideShowImageCapture

take photos of slideshow images on android phone or Canon EOS camera remotely using windows forms application

ANDROID CAMERA:
Application starts ADB(https://developer.android.com/studio/command-line/adb.html) connection with first connected android device and turns on the screen, opens OpenCamera app(http://opencamera.sourceforge.net/) that has to be preinstalled on your device and then starts full screen slideshow on secondary screen if one is connected or on primary screen otherwise. On every slideshow image transition Android OpenCamera is triggered and image is saved to device. When all slideshow images have been shown and captured, application closes OpenCamera app and exits! User can define timer for slideshow transition taking into account write speed of the device.

CANON CAMERA:
When application is used in Canon image capture mode, connection between app and camera is established. After that, full screen slideshow is started on secondary screen if one is connected or on primary screen otherwise. On every slideshow image transition Canon camera is triggered and image is saved to device(or host computer if user defined). When all slideshow images have been shown and captured, application terminates connection between camera and app and exits! User can define timer for slideshow transition taking into account write speed of the device.

SMARTEK CAMERA:
Application can be used with Smartek GigE cameras using SmartekAPI.
