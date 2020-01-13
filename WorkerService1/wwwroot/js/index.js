let username = null;

var hub_URL = "/mainHub";
var connection = new signalR
    .HubConnectionBuilder()
    .withUrl(hub_URL)
    .build();

window.onload = () => {
    connection.on("ShowMessage", function (json_data, type) {

        let messageObj = JSON.parse(json_data);
        let date_hms = messageObj.Date.split('T')[1].split('.')[0]; //example messageObj.Date = "2020-01-12T14:36:48.8633493+01:00"
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

        document.getElementById('coso')
            .innerHTML += "<p style='color:" + color + "'><i style='color:gray'>" + date_hms + "</i> [" + messageObj.User + "]: " + messageObj.Message + "</p > ";
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

    connection.onclose(() => {
        var status_label = document.getElementById('status');

        swap_item_status();
        status_label.style = 'color:red';
        status_label.innerText = "Disconnected";
    });

    document.getElementById('msg').onkeyup = (e) => {
        if (event.key === "Enter") {
            document.getElementById('btn').click();
        }
    };

    document.getElementById('btn').onclick = (e) => {
        let msg_field = document.getElementById('msg');

        connection.invoke("SendMessageFromClient", username, msg_field.value)
            .catch(err => console.error(err.toString()));

        msg_field.value = "";
    };

    document.getElementById('connect_btn').onclick = (e) => {
        var username_input = document.getElementById('username');

        if (username_input.value === "") {
            return;
        }

        swap_item_status();
        connection.start()
            .then(() => {
                console.log("Connected to " + hub_URL);
                connection.invoke("RegisterUser", username_input.value);
            });
    };

    document.getElementById('disconnect_btn').onclick = (e) => {
        connection.stop();
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