/**
    Sever for our word game,
**/


const PORT = process.env.PORT || 3000;
const socketIo = require("socket.io")(PORT);
const utils = require("./utils.js");
const spellchecker = require("spellchecker");

const { interval } = require('rxjs');
const { map, take } = require('rxjs/operators');

const GameActions = {
    init : "init",
    playerConnected : "playerConnected",
    playerDisconnected : "playerDisconnected",
    startGame : "startGame",
    wordSelected : "wordSelected",
    initAlphabet : "initAlphabet"
};

//console.log(`apple ${spellchecker.isMisspelled("apple")}`);
console.log(`server started {${PORT}}`);

const MIN_PLAYER_COUNT = 1;
const state = {};

socketIo.on("connection", socket => {

    const data = { 
                    id : utils.getRandomID(), 
                    name : utils.getRandomName(),
                    socket 
                };

    console.log(`client connected [${data.id} , ${data.name}]`);
    //for previous players
    socket.broadcast.emit(GameActions.playerConnected, { id : data.id, name : data.name });
    //for current player
    socket.emit(GameActions.init, { id : data.id, name : data.name });
    //simulate player connections for current player
    Object.keys(state).forEach(item => socket.emit(GameActions.playerConnected, { id : item.id, name : item.name }));
    state[data.id] = data;

    if(Object.keys(state).length == MIN_PLAYER_COUNT) {
        const event = { x : 2, char : 'Z' };
        console.log(`all players are ready, starting game with ${JSON.stringify(event)}`)
        Object.keys(state).forEach(player => {
            console.log(`starting game for ${player}, ${state[player].name}`);
            state[player].socket.emit(GameActions.startGame, event);
        });

        interval(2000).subscribe(counter => {
            const alphabet = { 
                id : counter,
                x : Math.floor(Math.random() * 5), 
                char : utils.getRandomChar() 
            };
            console.log(`sending ${JSON.stringify(alphabet)}`)
            Object.keys(state).forEach(player => {
                console.log(`sending to ${state[player].name}`);
                state[player].socket.emit(GameActions.initAlphabet, alphabet);
            });            
        });
    }

    socket.on("disconnect", () => {
        console.log(`client ${data.id} disconnected!!`);
        delete state[data.id];
        socket.broadcast.emit(GameActions.playerDisconnected, { id : data.id, name : data.name });
    });
});