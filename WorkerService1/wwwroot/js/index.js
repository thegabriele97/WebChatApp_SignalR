let username = null;

var hub_URL = "/mainHub";
var connection = new signalR
    .HubConnectionBuilder()
    .withUrl(hub_URL)
    .build();

window.onload = () => {
    connection.on("ShowMessage", function (user, message, type) {

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
            .innerHTML += "<p style='color:" + color + "'>[" + user + "]: " + message + "</p>";
    });

    connection.on("ConfirmUsername", (_username, confirmed) => {
        if (!confirmed) {
            alert("Username already in use!");
            document.getElementById('disconnect_btn').click();
        } else {
            var status_label = document.getElementById('status');

            username = _username;
            status_label.style = 'color:green';
            status_label.innerText = "Connected"
        }
    });

    document.getElementById('btn').onclick = (e) => {
        let msg = document.getElementById('msg').value;

        console.log("Sending message to server: " + msg);
        connection.invoke("SendMessageFromClient", username, msg)
            .catch(err => console.error(err.toString()));
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
        var username_input = document.getElementById('username');
        var status_label = document.getElementById('status');

        connection.stop();
        swap_item_status();
        status_label.style = 'color:red';
        status_label.innerText = "Disconnected";
    };
};

function swap_item_status() {
    let btn1 = document.getElementById('connect_btn');
    let btn2 = document.getElementById('disconnect_btn');
    let username_input = document.getElementById('username');

    btn1.disabled = !btn1.disabled;
    btn2.disabled = !btn2.disabled;
    username_input.disabled = !username_input.disabled;
}