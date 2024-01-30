function createTextboxes() {
    var numberOfTeams = document.querySelector('input[name="NumberOfTeams"]').value;
    var numberOfPosts = document.querySelector('input[name="NumberOfPosts"]').value;
    var container = document.getElementById("dynamicContainer");

    container.innerHTML = ""; // Clear existing textboxes

    for (var i = 0; i < numberOfTeams; i++) {
        var teamTextBox = document.createElement("input");
        teamTextBox.type = "text";
        teamTextBox.name = "Lag[" + i + "]";
        teamTextBox.placeholder = "Lag " + (i + 1);

        container.appendChild(teamTextBox);
        container.appendChild(document.createElement("br"));
    }

    for (var j = 0; j < numberOfPosts; j++) {
        var postTextBox = document.createElement("input");
        postTextBox.type = "text";
        postTextBox.name = "Posts[" + j + "]";
        postTextBox.placeholder = "Post-navn " + (j + 1);

        container.appendChild(postTextBox);
        container.appendChild(document.createElement("br"));
    }

    var nextButton = document.getElementById('nextButton');
    nextButton.value = 'Lag konkurranse';
    nextButton.disabled = true;

    // Enable "Lag konkurranse" button only if the specified conditions are met
    if (numberOfTeams >= 0 && numberOfPosts >= 0) {
        nextButton.disabled = false;
    }

    // Redirect to the admin page only if the button was clicked
    nextButton.addEventListener('click', function() {
        window.location.href = '/AdminPage';  // Update this URL with the actual URL of your admin page
    });
}

function updateSubmitButton() {
    var gameName = document.querySelector('input[name="GameName"]').value;
    var numberOfPosts = document.querySelector('input[name="NumberOfPosts"]').value;
    var numberOfTeams = document.querySelector('input[name="NumberOfTeams"]').value;

    var nextButton = document.getElementById('nextButton');
    nextButton.value = "Neste";

    if (gameName && numberOfPosts && numberOfTeams) {
        nextButton.disabled = false;
    } else {
        nextButton.disabled = true;
    }
}

document.getElementById('nextButton').addEventListener('click', function() {
    createTextboxes();
});