var playerName = ""; // Player's name (duh)
var playerCanPlay = false; // Whether the player can play or not
var playerLatestAction = ""; // Player's latest action (call/raise/fold)
var playerJustJoined = true; // Whether the player just joined

var pocketCards = []; // The player's cards
var pocketCardsCovered = true; // Whether the player's cards are covered (turned)

var tableCards = []; // The cards on the table

var pot = 0; // Pot amount
var fiches = 0; // Fiches amount

var minRaise = maxRaise = 0; // Minimum and maximum values for the Raise action

// UI Components
var actions = document.getElementById('actions');
var btnCall = document.getElementById('btnCall');
var btnRaise = document.getElementById('btnRaise');
var btnFold = document.getElementById('btnFold');
var btnDecreaseRaise = document.getElementById('decreaseRaise');
var btnIncreaseRaise = document.getElementById('increaseRaise');

var pocketCardsRow = document.getElementById("pocketCards");
var pocketCard1 = document.getElementById("pocketCard1");
var pocketCard2 = document.getElementById("pocketCard2");

var jsonCards = null;

/* ============= CLIENT UI ============= */
// Raise Slider component
$('#pokerSlider').slider({
    id: "raiseSlider",
    step: 2000,
    min: 0,
    max: 200000,
    value: 0,
    ticks_snap_bounds: 30,
    formatter: function(value) {
        return fichesConvert(value);
    }
});

$("#pokerSlider").on("slide", function(slideEvt) {
    $("#currentRaise").text(fichesConvert(slideEvt.value));
});

// Function: decrease slider's value by their step
function decreaseRaise() {
    newval = +$("#pokerSlider").val() - +$("#pokerSlider").data('slider').options.step;
    $("#pokerSlider").slider('setValue', newval);
}

// Function: increase slider's value by their step
function increaseRaise() {
    newval = +$("#pokerSlider").val() + +$("#pokerSlider").data('slider').options.step;
    $("#pokerSlider").slider('setValue', newval);
}

// Function: add a dot every three digits in a number (used on fiches strings)
function fichesConvert(fichesValue) {
    var fichesDots = fichesValue.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1.");
    return fichesDots;
}

// Function: update pot (both variable & on page)
function updatePot(newPot) {
    pot = newPot;
    document.getElementById('potAmount').innerHTML = fichesConvert(pot);
}

// Function: update fiches (both variable & on page)
function updateFiches(newFiches) {
    fiches = newFiches;
    //document.getElementById('fichesAmount').innerHTML = fichesConvert(fiches);
    document.getElementById('maxRaise').innerHTML = fichesConvert(fiches);
}

// Function: update call price (on page)
function updateCall(newCall) {
    document.getElementById('currentCall').innerHTML = fichesConvert(newCall);
}

// Function: update raise values (both variable & on page)
function updateRaise(newRaise) {
    // Change variables
    minRaise = newRaise.minRaise;
    maxRaise = newRaise.maxRaise;

    // Change values on page
    document.getElementById('minRaise').innerHTML = fichesConvert(minRaise);
    document.getElementById('maxRaise').innerHTML = fichesConvert(maxRaise);

    // Calculate new ticks
    // Avg between min and max, avg between min and new avg, avg between max and new avg
    var avg = (minRaise+maxRaise)/2;

    var ticks = [];
    ticks.push(minRaise,((minRaise+avg)/2),avg,((maxRaise+avg)/2),maxRaise);

    var ticks_labels = [];
    var i;
    for (i = 0; i < ticks.length; i++) {
        ticks_labels.push(ticks[i]);
    }

    // New interval (step)
    var step = maxRaise/10;

    // Change values on slider
    $("#pokerSlider").slider('setAttribute', 'min', minRaise);
    $("#pokerSlider").slider('setAttribute', 'max', maxRaise);
    $("#pokerSlider").slider('setAttribute', 'step', step);
    $("#pokerSlider").slider('setAttribute', 'ticks', ticks);
    $("#pokerSlider").slider('setAttribute', 'ticks_labels', ticks_labels);
    // Reset value
    $("#pokerSlider").slider('setValue', avg);
    // Refresh slider
    $("#pokerSlider").slider('refresh');
}

// Function: cover table cards all at once
function coverTablecards() {
    document.getElementById('tableCard1').classList.remove('flip-card-rotated');
    document.getElementById('tableCard2').classList.remove('flip-card-rotated');
    document.getElementById('tableCard3').classList.remove('flip-card-rotated');
    document.getElementById('tableCard4').classList.remove('flip-card-rotated');
    document.getElementById('tableCard5').classList.remove('flip-card-rotated');
}

// Function: hide pocket cards
function hidePocket() {
    pocketCardsRow.classList.add('hidden');
}

// Function: show pocket cards (unhide, don't uncover)
function unhidePocket() {
    pocketCardsRow.classList.remove('hidden');
 //   pocketCardsRow.fadeIn();
}

// Function: hide actions
function hideActions() {
    actions.classList.add('hidden');
}

// Function: show actions
function unhideActions() {
    actions.classList.remove('hidden');
//    actions.fadeIn();
}

// Function: disable actions
function disableActions() {
    btnCall.disabled = true;
    btnRaise.disabled = true;
    btnFold.disabled = true;
    btnDecreaseRaise.disabled = true;
    btnIncreaseRaise.disabled = true;
}

// Function: enable actions (and reset latest action)
function enableActions() {
    btnCall.disabled = false;
    btnRaise.disabled = false;
    btnFold.disabled = false;
    btnDecreaseRaise.disabled = false;
    btnIncreaseRaise.disabled = false;

    btnCall.classList.remove('active');
    btnRaise.classList.remove('active');
    btnFold.classList.remove('active');
}

// Function: set latest action
function setLatestAction(action) {
    switch (action) {
        case 'Call':
            btnCall.classList.add('active');
            break;
        case 'Raise':
            btnRaise.classList.add('active');
            break;
        case 'Fold':
            btnFold.classList.add('active');
            break;
    }
}

// Function: update pocket cards (cover them)
function updatePocket(newCards) {
    // Cover cards and show them
    coverCards();
    unhidePocket();
    // Reset current pocket
    pocketCards = [];
    // Determine cards and insert them into array
    var i;
    for (i = 0; i < 2; i++) {
        var card = '';
        switch (newCards.cards[0][i]) {
            case 11:
    //            card.concat('J');
                card+='J';
                break;
            case 12:
    //            card.concat('Q');
                card+='Q';
                break;
            case 13:
    //            card.concat('K');
                card+='K';
                break;
            case 14:
    //            card.concat('A');
                card+='A';
                break;
            default:
    //            card.concat(newCards.cards[i][0]);
                card+=newCards.cards[i][0];
                break;
        }
        switch (newCards.cards[1][i]) {
            case 1:
    //            card.concat('D');
                card+='D';
                break;
            case 2:
    //            card.concat('C');
                card+='C';
                break;
            case 3:
    //            card.concat('H');
                card+='H';
                break;
            case 4:
    //            card.concat('S');
                card+='S';
                break;
        }
        pocketCards.push(card);
    }
    // Change their pictures
    pocketCard1.getElementsByTagName('img')[1].src='../img/pokerGame/' + pocketCards[0]+".png";
    pocketCard2.getElementsByTagName('img')[1].src='../img/pokerGame/' + pocketCards[1]+".png";

    ShowCards();
}

// Slider


/* ============= CALLS ============= */ 
/* Functions to manage server calls (read 'Server -> Frontend calls') */
// Function: initialize fiches; show cards & pot if game is already on; save player name
function JoinTable(json) {

    // Hide pocket cards
    hidePocket();
    // Hide actions
    hideActions();
    // Save player name
    playerName = json.playerName;
    // Update fiches
    updateFiches(json.fiches);
    // Update pot
    updatePot(json.pot);
    // Show table cards
    tableCards = json.tableCards;
    // Check if they joined mid-game and are waiting for their turn (show message in that case)
    if (json.pot == 0) document.getElementById('textWait').classList.remove('hidden');
};

// Function: get cards (don't show yet)
function giveCards(json) {
    jsonCards = json
    $( document ).ready(function() {
        var json = JSON.parse(jsonCards);
        updatePocket(json);
    });
};

// Function: show pocket cards
function ShowCards() {
    pocketCardsCovered = false;
    pocketCard1.classList.add('flip-card-rotated');
    pocketCard2.classList.add('flip-card-rotated');
};

// Function: cover pocket cards
function coverCards() {
    pocketCardsCovered = true;
    pocketCard1.classList.remove('flip-card-rotated');
    pocketCard2.classList.remove('flip-card-rotated');
};

// Function: get new table card and show it
function addTableCard(json) {
    var json = JSON.parse(json);

    // Determine card
    var card;
    switch (json.card) {
        case 11:
            card.concat('J');
            break;
        case 12:
            card.concat('Q');
            break;
        case 13:
            card.concat('K');
            break;
        case 14:
            card.concat('A');
        default:
            card.concat(json.card);
            break;
    }
    
    switch (json.seed) {
        case 1:
            card.concat('D');
            break;
        case 2:
            card.concat('C');
            break;
        case 3:
            card.concat('H');
            break;
        case 4:
            card.concat('S');
            break;
    }

    tableCards.push(card);

    switch(tableCards.length) {
        case 1:
            document.getElementById('tableCard1-img').src = "../img/pokerGame/" + card + ".png";
            document.getElementById('tableCard1').classList.add('flip-card-rotated');
            break;
        case 2:
            document.getElementById('tableCard2-img').src = "../img/pokerGame/" + card + ".png";
            document.getElementById('tableCard2').classList.add('flip-card-rotated');
            break;
        case 3:
            document.getElementById('tableCard3-img').src = "../img/pokerGame/" + card + ".png";
            document.getElementById('tableCard3').classList.add('flip-card-rotated');
            break;
        case 4:
            document.getElementById('tableCard4-img').src = "../img/pokerGame/" + card + ".png";
            document.getElementById('tableCard4').classList.add('flip-card-rotated');
            break;
        case 5:
            document.getElementById('tableCard5-img').src = "../img/pokerGame/" + card + ".png";
            document.getElementById('tableCard5').classList.add('flip-card-rotated');
            break;
    }

    // Animate card (move and turn around)
};

// Function: update plate along with the player's latest action (call/raise/fold)
function playerPlayed(json) {
    var json = JSON.parse(json);

    // Update pot
    updatePot(json.updatedPot);
    // Highlight latest action
    playerLatestAction = json.action;
    setLatestAction(playerLatestAction);
    // Player cannot play
    playerCanPlay = false;
};

// Function: allow player to play (their turn now); show action buttons
function OnPlayerTurn(json) {
    // Update call price value
    updateCall(json.call);
    // Don't highlight latest action anymore
    unhideActions();
    enableActions();
    // Let player play
    playerCanPlay = true;
    // Remove waiting message if it's there
    document.getElementById('textWait').classList.remove('hidden');
};

// Function: change min and max values of Raise
function updateRaiseLimits(json) {
    var json = JSON.parse(json);
    
    updateRaise(json);
};

// Function: reset winning text
function StartNextMatch() {
    document.getElementById('textWait').classList.add('hidden');
    document.getElementById('textVictory').classList.add('hidden');
    document.getElementById('textDefeat').classList.add('hidden');
    document.getElementById('textNextRound').classList.add('hidden');
};

// Function: manage match completed (show winner name, win/lose message, reset match)
function matchCompleted(json) {
    var json = JSON.parse(json);
    // Check if the player is the winner and show win/lose message
    if (json.winnerName == playerName) {
        // Player is the winner
        document.getElementById('fichesWon').innerHTML = pot;
        document.getElementById('textVictory').classList.remove('hidden');
    } else {
        // Player is not the winner
        document.getElementById('defeatName').innerHTML = json.winnerName;
        document.getElementById('defeatFiches').innerHTML = pot;
        document.getElementById('textDefeat').classList.remove('hidden');
    }
    // Reset match (JS)
    pocketCards = [];
    pocketCardsCovered = true;
    tableCards = [];
    updatePot(0);
    updateFiches(0);
    // Reset match (UI)
    hidePocket();
    coverCards();
    coverTablecards();
    // Enable and reset them back again to remove the latest action
    enableActions();
    disableActions();
    // Start time and update seconds counter
    document.getElementById('textNextRound').classList.remove('hidden');
    var timeleft = 10;
    var downloadTimer = setInterval(function() {
        if(timeleft <= 0) {
            clearInterval(downloadTimer);
            document.getElementById("replayTimer").innerHTML = "0 secondi.";
        } else if (timeleft == 1) {
            document.getElementById("replayTimer").innerHTML = "1 secondo.";
        } else {
            document.getElementById("replayTimer").innerHTML = timeleft + " secondi.";
        }
        timeleft -= 1;
    }, 1000);
};

/* Server -> Frontend calls */
// Call: player joins table
mp.events.add('JoinTable', (json) => {
    JoinTable(json);
});

// Call: player gets pocket cards
mp.events.add('GiveCards', (json) => {
    giveCards(json);
});

// Call: player's pocket cards are shown to them
mp.events.add('ShowCards', () => {
    ShowCards();
});

// Call: card is added to the table
mp.events.add('AddTableCard', (json) => {
    addTableCard(json);
});

// Call: update pot and player's latest action
mp.events.add('OnPlayerPlayed', (json) => {
    playerPlayed(json);
});

// Call: allow player to play
mp.events.add('OnPlayerTurn', (json) => {
    OnPlayerTurn(json);
});

// Call: update player's minimum and maximum raise amounts
mp.events.add('OnPlayerRaiseUpdated', (json) => {
    updateRaiseLimits(json);
});

// Call: manage UI and reset everything upon completed match
mp.events.add('OnMatchCompleted', (json) => {
    matchCompleted(json);
});

// Call: manage UI after new match
mp.events.add('StartNextMatch', () => {
    StartNextMatch();
});

/* Frontend -> Server functions */
// Function: player calls during their turn
function pokerCall() {
    if (playerCanPlay) {
        mp.events.call('PokerCallEvent');
        playerCanPlay = false;
    }
}

// Function: player raises during their turn (gets fiches amount from slider value)
function pokerRaise() {
    if (playerCanPlay) {
        fiches = $('#pokerSlider').data('slider').getValue();
        mp.events.call('PokerRaiseEvent', fiches);
        playerCanPlay = false;
    }
}

// Function: player folds during their turn
function pokerFold() {
    if (playerCanPlay) {
        mp.events.call('PokerFoldEvent');
        playerCanPlay = false;
    }
}

// Function: player leaves game
function pokerLeave() {
    $.confirm({
        closeAnimation: 'scale',
        closeIcon: true,
        title: 'Attento!',
        content: 'Sei sicuro di voler abbandonare la partita di Poker?',
        buttons: {
            'confirm': {
                text: 'ESCI',
                btnClass: 'btn-danger',
                action: function(){
                    mp.events.call('PokerLeaveEvent');
                }
            },
            'cancel': {
                text: 'ANNULLA',
            }
        },
        theme: 'material',
        type: 'blue',
    });
}
