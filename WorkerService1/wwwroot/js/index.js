let username = null;

var hub_URL = "/mainHub";
var connection = new signalR
    .HubConnectionBuilder()
    .withUrl(hub_URL)
    .build();

window.onload = () => {
    connection.on("ShowMessage", function (json_data) {

        let messageObj = JSON.parse(json_data);
        let date_hms = messageObj.Date.split('T')[1].split('.')[0]; //example messageObj.Date = "2020-01-12T14:36:48.8633493+01:00"
        let color = "black";

        writeMessageInChat(messageObj.User.Username, messageObj.Message, messageObj.Type, date_hms);
        if (messageObj.Type === 1) {
            connection.invoke('GetNumberOfActiveUsers');
        }
    });

    connection.on("ConfirmUsername", (_username, confirmed) => {
        if (!confirmed) {
            alert("Username already in use!");
            document.getElementById('disconnect_btn').click();
        } else {
            var status_label = document.getElementById('status');

            username = _username;
            status_label.style = 'color:green';
            status_label.innerText = "Connected";
        }
    });

    connection.on("SendNumberOfActiveUsers", (raw_data) => {
        document.getElementById('label_activeUsers')
            .innerText = JSON.parse(raw_data).count_users;
    });

    connection.onclose(doLastThingToCloseConn);

    document.getElementById('msg').onkeyup = (e) => {
        if (event.key === "Enter") {
            document.getElementById('btn').click();
        }
    };

    document.getElementById('username').onkeyup = (e) => {
        if (event.key === "Enter") {
            document.getElementById('connect_btn').click();
        }
    };

    document.getElementById('btn').onclick = (e) => {
        let msg_field = document.getElementById('msg');

        connection.invoke("SendMessageFromClient", username, msg_field.value)
            .catch(err => {
                console.error(err.toString());
                writeMessageInChat("Client", err, 2);
            });

        msg_field.value = "";
    };

    document.getElementById('connect_btn').onclick = (e) => {
        var username_input = document.getElementById('username');
        var status_label = document.getElementById('status');

        if (username_input.value === "") {
            return;
        }

        swap_item_status();
        status_label.style = 'color:blue';
        status_label.innerText = "Connecting..";

        connection.start()
            .then(() => {
                console.log("Connected to " + hub_URL);

                var HTTPConn = new XMLHttpRequest();
                HTTPConn.open("GET", "https://api.ipify.org/?format=json");
                HTTPConn.onreadystatechange = () => {
                    if (HTTPConn.readyState === 4 && HTTPConn.status === 200) {
                        let response_obj = JSON.parse(HTTPConn.responseText);
                        connection.invoke("RegisterUser", username_input.value, response_obj.ip);
                    }
                };

                HTTPConn.send();
            })
            .catch(doLastThingToCloseConn);
    };

    document.getElementById('disconnect_btn').onclick = (e) => {
        connection.stop();

        if (username !== null) {
            writeMessageInChat("Server", "Bye, see you son!", 1);
        }
    };
};

function swap_item_status() {
    let btn1 = document.getElementById('connect_btn');
    let btn2 = document.getElementById('disconnect_btn');
    let username_input = document.getElementById('username');
    let btn_send_msg = document.getElementById('btn');
    let msg_input = document.getElementById('msg');

    btn1.disabled = !btn1.disabled;
    btn2.disabled = !btn2.disabled;
    username_input.disabled = !username_input.disabled;
    btn_send_msg.disabled = !btn_send_msg.disabled;
    msg_input.disabled = !msg_input.disabled;
}

function doLastThingToCloseConn() {
    var status_label = document.getElementById('status');

    swap_item_status();
    status_label.style = 'color:red';
    status_label.innerText = "Disconnected";
    document.getElementById('label_activeUsers').innerText = 'NaN';
    username = null;
}

function writeMessageInChat(user, message, type, date_hms = null, element_id = 'coso') {

    if (date_hms === null) {
        let date = new Date();
        date_hms = date.getHours() + ":" + date.getMinutes() + ":" + date.getSeconds();
    }

    let color = "black";
    switch (type) {
        case 1:
            color = "green";
            break;
        case 2:
            color = "red";
            break;
        case 3:
            color = "blue";
            break;
    }

    document.getElementById(element_id)
        .innerHTML += "<p style='color:"
            + color + "'><i style='color:gray'>"
            + date_hms + "</i> [" + user + "]: "
            + message + "</p > ";
}