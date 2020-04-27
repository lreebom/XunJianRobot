var ROSLIB = {

    ROS_Init: function () {
        console.log("ROS_Init Start");

        var initCount = 0;

        var eventemitter2Script = document.createElement("script")
        eventemitter2Script.type = "text/javascript";
        eventemitter2Script.onload = function () {
            initCount++;
            if (initCount >= 2) {
                callUnityFunction('OnLibraryInitialized', '');
            }
        };
        eventemitter2Script.src = '/static/webgl/scripts/eventemitter2.min.js';
        // eventemitter2Script.src = './scripts/eventemitter2.min.js';
        document.getElementsByTagName("head")[0].appendChild(eventemitter2Script);

        var rosScript = document.createElement("script")
        rosScript.type = "text/javascript";
        rosScript.onload = function () {
            initCount++;
            if (initCount >= 2) {
                callUnityFunction('OnLibraryInitialized', '');
            }
        };
        rosScript.src = '/static/webgl/scripts/roslib.min.js';
        // rosScript.src = './scripts/roslib.min.js';
        document.getElementsByTagName("head")[0].appendChild(rosScript);
    },

    ROS_Connect: function () {

        var ConnectError = 1;
        var ConnectSuccess = 0;
        var Connecting = 1;
        var ConnectClose = 2;

        // Connecting to ROS
        // -----------------
        ROS1 = new ROSLIB.Ros();
        console.log("ROS1 Init", ROS1);

        // If there is an error on the backend, an 'error' emit will be emitted.
        ROS1.on('error', function (error) {
            console.log("ROS1 error:", error);
            callUnityFunction("GetConnectState", ConnectError);
        });

        // // Find out exactly when we made a connection.
        ROS1.on('connection', function () {
            console.log('ROS1 Connection made!');
            callUnityFunction("GetConnectState", ConnectSuccess);


        });

        ROS1.on('close', function () {
            console.log('ROS1 Connection closed.');
            callUnityFunction("GetConnectState", ConnectClose);
            setTimeout(function () {
                ROS1.connect(ROS1_ADDR);
                callUnityFunction("GetConnectState", Connecting);
            }, 1000);
        });

        console.log("ROS1 开始连接：" + ROS1_ADDR);
        // Create a connection to the rosbridge WebSocket server.
        ROS1.connect(ROS1_ADDR);

        callUnityFunction("GetConnectState", Connecting);

        // get position information
        var listener = new ROSLIB.Topic({
            ros: ROS1,
            name: '/ucr_control/Odom',
            messageType: 'nav_msgs/Odometry'
        });

        listener.subscribe(function (message) {
            var currentPos = {
                x: message.pose.pose.position.x,
                y: message.pose.pose.position.y,
                z: message.pose.pose.position.z,
            };
            console.log('ROS1 currentPos= ', currentPos);
            callUnityFunction("GetRobotPosition", JSON.stringify(currentPos));
            // listener.unsubscribe();
        });

        //get camera ptz(水平转角、垂直转角、缩放Zoom)
        var listener2 = new ROSLIB.Topic({
            ros: ROS1,
            name: '/camera_position',
            messageType: 'robot_srv/camera_ptz'
        });

        listener2.subscribe(function (message) {
            var currentRot = {
                p: message.p_pos / 10.0,
                t: message.t_pos / 10.0,
                z: message.z_pos / 10.0
            };
            console.log('ROS1 currentRot= ', currentRot);
            callUnityFunction("GetCameraRotation", JSON.stringify(currentRot));
            // listener2.unsubscribe();
        });

        //回零
        var listener3 = new ROSLIB.Topic({
            ros: ROS1,
            name: '/ucr_command',
            messageType: 'std_msgs/Int32MultiArray'
        });

        //判断message.data[1]==4,若成立则回零,否则没有回零
        listener3.subscribe(function (message) {
            console.log("ROS1  回零对应的操作 ", message);
            if (message.data[1] == 4) {
                callUnityFunction("CallText1", "Start 回零");

                switch (message.data[2]) {
                    case 0:
                        callUnityFunction("BackZero", "xz");
                        break;
                    case 1:
                        callUnityFunction("BackZero", "x");
                        break;
                    case 2:
                        callUnityFunction("BackZero", "z");
                        break;
                    default:
                        callUnityFunction("BackZero", "none");
                        break;
                }

            }
        });

        //控制机器人,伸缩杆运动
        ROS1_MoveService = new ROSLIB.Service({
            ros: ROS1,
            name: '/control_to_move',
            serviceType: 'robot_srv/move',
        });
        console.log("init ROS1_MoveService ", ROS1_MoveService);
        ROS1_MoveRequest = new ROSLIB.ServiceRequest({
            isMove: false,
            x_speed: 0.0,
            // y_speed: 0.0,
            z_speed: 0.0
        });
        console.log("init ROS1_MoveRequest ", ROS1_MoveRequest);

        //球头控制
        ROS1_CameraService = new ROSLIB.Service({
            ros: ROS1,
            name: 'PTZ_Control',
            serviceType: 'robot_srv/ptz_control'
        });

        ROS1_CameraRequest = new ROSLIB.ServiceRequest({
            PTZCommand: 23,
            Stop: 1,
            Speed: 3
        });

        //归零
        ROS1_ZeroService = new ROSLIB.Service({
            ros: ROS1,
            name: '/ucr_control/all_to_home',
            serviceType: 'std_srvs/Trigger'
        });

        ROS1_ZeroRequest = new ROSLIB.ServiceRequest({

        });

    },

    ROS1_Move: function (isMove, xSpeed, ySpeed, zSpeed) {
        //开始运动,x_speed是左右移动速度,z_speed是伸缩速度,下例为x正方向0.2速度运动,isMove=true为开始运动,isMove=false为停止
        ROS1_MoveRequest.isMove = isMove === 1 ? true : false;
        ROS1_MoveRequest.x_speed = xSpeed;
        // ROS1_MoveRequest.y_speed = ySpeed;
        ROS1_MoveRequest.z_speed = zSpeed;
        console.log("发送移动指令:", ROS1_MoveService, ROS1_MoveRequest);
        ROS1_MoveService.callService(ROS1_MoveRequest, function (result) {
            console.log("ROS1_MoveRequest Result:", result);
        });
    },

    ROS1_Camera: function (stop, command, speed) {
        // //开始控制,PTZCommand为控制模式:23为左转,24为右转,21为上仰,22为下俯,11为焦距变大,12为焦距变小.Stop=1为停止,Stop=0为开始运动,Speed为运动速度,分为1-6.下例为以4的速度,右转
        ROS1_CameraRequest.PTZCommand = command;
        ROS1_CameraRequest.Stop = stop === 1 ? 1 : 0;
        ROS1_CameraRequest.Speed = speed;
        console.log("发送Camear指令:", ROS1_CameraService, ROS1_CameraRequest);
        ROS1_CameraService.callService(ROS1_CameraRequest, function (result) {
            console.log("ROS1_CameraRequest Result:", result);
        });
    },

    ROS1_Zero: function () {
        console.log("发送归零指令:", ROS1_ZeroService, ROS1_ZeroRequest);
        ROS1_ZeroService.callService(ROS1_ZeroRequest, function (result) {
            console.log("ROS1_Zero Result:", result);
            callUnityFunction("ROS1_BackZeroResult");
        });
    },

    ROS_Connect2: function () {
        console.log("ROS_Connect2", ROS2_ADDR);

        var ConnectError = 1;
        var ConnectSuccess = 0;
        var Connecting = 1;
        var ConnectClose = 2;

        // Connecting to ROS
        // -----------------
        var ros2 = new ROSLIB.Ros();
        console.log("ros2", ros2);

        // If there is an error on the backend, an 'error' emit will be emitted.
        ros2.on('error', function (error) {
            console.log("error:", error);
            callUnityFunction("GetConnectState2", ConnectError);
        });

        // // Find out exactly when we made a connection.
        ros2.on('connection', function () {
            console.log('Connection made!');
            callUnityFunction("GetConnectState2", ConnectSuccess);
        });

        ros2.on('close', function () {
            console.log('机器人2 Connection closed.');
            callUnityFunction("GetConnectState2", ConnectClose);
            setTimeout(function () {
                ros2.connect(ROS2_ADDR);
                callUnityFunction("GetConnectState2", Connecting);
            }, 1000);
        });

        console.log("机器人2 开始连接：" + ROS2_ADDR);
        // Create a connection to the rosbridge WebSocket server.
        ros2.connect(ROS2_ADDR);

        callUnityFunction("GetConnectState2", Connecting);

        // get position information
        var listenerR2 = new ROSLIB.Topic({
            ros: ros2,
            name: '/ucr_control/Odom',
            messageType: 'nav_msgs/Odometry'
        });

        listenerR2.subscribe(function (message) {
            var currentPos = {
                x: message.pose.pose.position.x,
                y: message.pose.pose.position.y,
                z: message.pose.pose.position.z,
            };
            console.log('机器人2 currentPos= ', currentPos);
            callUnityFunction("GetRobotPosition2", JSON.stringify(currentPos));
            // listener.unsubscribe();
        });

        //get camera ptz(水平转角、垂直转角、缩放Zoom)
        var listener2R2 = new ROSLIB.Topic({
            ros: ros2,
            name: '/camera_position',
            messageType: 'robot_srv/camera_ptz'
        });

        listener2R2.subscribe(function (message) {
            var currentRot = {
                p: message.p_pos / 10.0,
                t: message.t_pos / 10.0,
                z: message.z_pos / 10.0
            };
            console.log('机器人2 currentRot= ', currentRot);
            callUnityFunction("GetCameraRotation2", JSON.stringify(currentRot));
            // listener2.unsubscribe();
        });

        //回零
        var listener3R2 = new ROSLIB.Topic({
            ros: ros2,
            name: '/ucr_command',
            messageType: 'std_msgs/Int32MultiArray'
        });

        //判断message.data[1]==4,若成立则回零,否则没有回零
        listener3R2.subscribe(function (message) {
            console.log("机器人2 回零对应的操作 ", message);
            if (message.data[1] == 4) {
                callUnityFunction("CallText2", "Start 回零");

                switch (message.data[2]) {
                    case 0:
                        callUnityFunction("BackZero2", "xz");
                        break;
                    case 1:
                        callUnityFunction("BackZero2", "x");
                        break;
                    case 2:
                        callUnityFunction("BackZero2", "z");
                        break;
                    default:
                        callUnityFunction("BackZero2", "none");
                        break;
                }

            }

        });
    }
};

mergeInto(LibraryManager.library, ROSLIB);