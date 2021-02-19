import { Client } from "colyseus";
import { ColyseusRoomState } from "../schema/ColyseusRoomState";
import { ShootingGalleryRoom } from "../ShootingGalleryRoom";

/** Reference the target data */
import Targets from "./targets";

const logger = require("../../helpers/logger");

// string identifiers for keys in attributes
const CurrentState = "currentGameState";
const LastState = "lastGameState";
const GeneralMessage = "generalMessage";
const ClientReadyState = "readyState";
const BeginRoundCountDown = "countDown";

/** Enum for game state */
const ServerGameState = {
    None: "None", 
    Waiting: "Waiting",// When a room first starts up this is the state it will be in
    SendTargets: "SendTargets",
    BeginRound: "BeginRound",
    SimulateRound: "SimulateRound",
    EndRound: "EndRound"
};

/** Enum for begin round count down */
const CountDownState = {
    Enter: "Enter",
    GetReady: "GetReady",
    CountDown: "CountDown"
};

//Type safe Target wrapper
interface TargetObject { id : -1, name: "", value : -1, uid: "", claimed: false, row: -1}

/** Count down time before a round begins */
const CountDownTime = 3;

/** Reference to the room options that get passed in at time of Initialization. */
let roomOptions: any;

/**
 * The primary game loop on the server
 * @param {*} roomRef Reference to the Room object
 * @param {*} deltaTime Expects deltaTime to be in seconds, not milliseconds
 */
let gameLoop = function (roomRef: ShootingGalleryRoom, deltaTime: number){

    // Update the game state
    switch(getGameState(roomRef, CurrentState)){
        case ServerGameState.None:
            break;
        case ServerGameState.Waiting:
            waitingLogic(roomRef, deltaTime);
            break;
        case ServerGameState.SendTargets:
            sendTargetsLogic(roomRef, deltaTime);
            break;
        case ServerGameState.BeginRound:
            beginRoundLogic(roomRef, deltaTime);
            break;
        case ServerGameState.SimulateRound:
            simulateRoundLogic(roomRef, deltaTime);
            break;
        case ServerGameState.EndRound:
            endRoundLogic(roomRef, deltaTime);
            break;
        default:
            logger.error(`Unknown Game State - ${getGameState(roomRef, CurrentState)}`);
            break;
    }
}

// Client Request Logic
// These functions get called by the client in the form of the "customMethod" message set up in the room.
//======================================
/**
 * Track a client scoring a target
 * @param {*} roomRef Reference to the room
 * @param {*} client The Client reporting the target score
 * @param {*} request Data including which client and which target
 */
const customMethods: any = {};
customMethods.scoreTarget = function (roomRef: ShootingGalleryRoom, client: Client, request: any) {

    if(getGameState(roomRef, CurrentState) != ServerGameState.SimulateRound) {
        logger.silly("Cannot score a target until the game has begun!");
        return;
    }

    const param = request.param;

    // 0 = entity id | 1 = target uid
    if(param == null || param.length < 2){
        throw "Score Target - Missing parameter";
        return;
    }

    // Entity ID
    const entityID = param[0];
    // Target UID
    const targetUID = param[1];

    if(roomRef.state.networkedEntities.has(entityID)){
        // Entity exists

        // Check if target exists and has not yet been claimed
        if (roomRef.currentTargetSet.has(targetUID) && roomRef.currentActiveTargets.has(targetUID)) {
            let target = roomRef.currentTargetSet.get(targetUID);
            if(target.claimed == false){
                // target is unclaimed

                // Remove the target from active target collection
                roomRef.currentActiveTargets["delete"](targetUID);

                // Update the entity's score
                scoreTargetForEntity(roomRef, entityID, target);
            }
            else{
                // target has already been claimed by another user
                logger.silly(`Target has already been claimed! - ${targetUID}`);
            }
        }
        else {
            logger.silly(`Missing target or target already claimed - ${targetUID}`);
        }
    }
    else{
        logger.error(`No Entity with ID: ${entityID}`)
    }
}
//====================================== END Client Request Logic

// GAME LOGIC
//======================================
/**
 * Update the score of an entity.
 * The update can be either +/-
 * @param {*} roomRef Reference to the room
 * @param {*} entityID ID of the entity whose score is getting updated
 * @param {*} value Value of the score update
 */
let scoreTargetForEntity = function(roomRef: ShootingGalleryRoom, entityID: string, target: any) {
    // already verified that the entity exists

    if(roomRef.state.networkedEntities.has(entityID) == false){
        logger.error(`Can't score target for entity Id \"${entityID}\" it isn't in the room.`);
    }

    // Claim the target
    target.claimed = true;

    if (roomRef.gameScores.has(entityID)) {
        let entityScore = roomRef.gameScores.get(entityID);
        entityScore += Number(target.value);
        roomRef.gameScores.set(entityID, entityScore);
    }
    else {
        roomRef.gameScores.set(entityID, Number(target.value));
    }

    roomRef.broadcast("onScoreUpdate", { entityID, targetUID: target.uid, score: roomRef.gameScores.get(entityID) });
}

/**
 * Returns a random set of targets.
 * @param {*} numOfTargets The number of desired targets 
 */
let getTargetSet = function(roomRef: ShootingGalleryRoom, numOfTargets: number) {
    
    const numberOfTargets = Number(numOfTargets);
    if(isNaN(numberOfTargets)){
        throw `Invalid Number of Targets - ${numOfTargets}`;
        return;
    }
    //Get the number of rows that the client has ready for targets
    let numberOfRows = Number(roomOptions["numberOfTargetRows"] || 4);
    // Get the random set of targets
    const lineUp = Targets.getRandomTargetLineUp(numberOfTargets, numberOfRows);
    // Build the map of current targets
    //====================================
    roomRef.currentTargetSet.clear();
    roomRef.currentActiveTargets.clear();

    for(let i = 0; i < lineUp.length; i++){
        if(roomRef.currentTargetSet.has(lineUp[i].uid) == false){
            roomRef.currentTargetSet.set(lineUp[i].uid, lineUp[i]);
        }

        if(roomRef.currentActiveTargets.has(lineUp[i].uid) == false){
            roomRef.currentActiveTargets.set(lineUp[i].uid, lineUp[i]);
        }
    }
    //====================================

    return lineUp;
}

/**
 * Checks if all the connected clients have a 'readyState' of "ready"
 * @param {*} users The collection of users from the room's state
 */
let checkIfUsersReady = function(users: ColyseusRoomState['networkedUsers']) {
    let playersReady = true;
    for(let entry of Array.from(users.entries())) {
        let readyState = entry[1].attributes.get(ClientReadyState);

        if(readyState == null || readyState != "ready"){
            playersReady = false;
            break;
        }
    }

    return playersReady;
}

/**
 * Sets attribute of all connected users.
 * @param {*} roomRef Reference to the room
 * @param {*} key The key for the attribute you want to set
 * @param {*} value The value of the attribute you want to set
 */
 let setUsersAttribute = function(roomRef: ShootingGalleryRoom, key: string, value: string) {
    
    roomRef.state.networkedUsers.forEach((userValue, userKey) => {
        let msg: any = {userId: userKey, attributesToSet: {}};

        msg.attributesToSet[key] = value;

        (roomRef as any).setAttribute(null, msg);
    });
    
}

/**
 * Sets attriubte of the room
 * @param {*} roomRef Reference to the room
 * @param {*} key The key for the attribute you want to set
 * @param {*} value The value of the attribute you want to set
 */
let setRoomAttribute = function(roomRef: ShootingGalleryRoom, key: string, value: string) {
    roomRef.state.attributes.set(key, value);
}

/**
 * Returns the game state of the server
 * @param {*} roomRef Reference to the room
 * @param {*} gameState Key for which game state you want, either the Current game state for the Last game state
 */
let getGameState = function (roomRef: ShootingGalleryRoom, gameState: string) {

    return roomRef.state.attributes.get(gameState);
}

/**
 * Returns an object that includes the winner Id, the winning score, or
 * an array of entity Ids of those that tied for first place
 * @param {*} roomRef Reference to the room
 */
let getRoundWinner = function (roomRef: ShootingGalleryRoom) {

    const winner: any = { id: "", score: 0, tie: false, tied: [] };
    if(getGameState(roomRef, CurrentState) != ServerGameState.EndRound){
        logger.error("Can't determine winner yet! Not in End Round");
        winner.id = "TBD";
        return winner;
    }

    // Used to determine if any players tied for highest score
    let scoreMap: any = {};

    try {
        // Iterate to find biggest score
        roomRef.gameScores.forEach((score, player) => {
            // If the player is not in the room then skip them
            if(!roomRef.state.networkedEntities.has(player)) {
                return;
            }

            // Add score and player Id to the score map
            if(scoreMap[score] == null)
                scoreMap[score] = [player];
            else
                scoreMap[score].push(player);

            // Set new winner and score
            if(score > winner.score){
                winner.id = player;
                winner.score = score;
            }
        });

        // Check for tied players
        if(scoreMap == undefined || winner.score == undefined || scoreMap[winner.score] == undefined) {
            logger.error("Failed to get scoreMap or winner");
        }
        if(scoreMap[winner.score].length > 1) {
            winner.id = "It's a tie!";
            winner.tie = true;
            winner.tied = scoreMap[winner.score];
        }
    } catch(error) {
        logger.error("Failed in getRoundWinner");
        logger.error(error);
    }
    return winner;
}

/** Resets data tracking collection and unlocks the room */
let resetForNewRound = function (roomRef: ShootingGalleryRoom) {
    roomRef.gameScores.clear();
    roomRef.currentTargetSet.clear();
    roomRef.currentActiveTargets.clear();

    setUsersAttribute(roomRef, ClientReadyState, "waiting");
    unlockIfAble(roomRef);
}

let unlockIfAble = function (roomRef: ShootingGalleryRoom) {
    if(roomRef.hasReachedMaxClients() === false) {
        roomRef.unlock();
    }
}
//======================================

// GAME STATE LOGIC
//======================================
/**
 * Move the server game state to the new state
 * @param {*} roomRef Reference to the room
 * @param {*} newState The new state to move to
 */
let moveToState = function (roomRef: ShootingGalleryRoom, newState: string) {
    // LastState = CurrentState
    setRoomAttribute(roomRef, LastState, getGameState(roomRef, CurrentState));
            
    // CurrentState = newState
    setRoomAttribute(roomRef, CurrentState, newState);
}

/**
 * The logic run when the server is in the Waiting state
 * @param {*} roomRef Reference to the room
 * @param {*} deltaTime Server delta time in seconds
 */
let waitingLogic = function (roomRef: ShootingGalleryRoom, deltaTime: number) {
    
    let playersReady = false;
    // Switch on LastState since the waiting logic gets used in multiple places
    switch(getGameState(roomRef, LastState)){
        case ServerGameState.None:
        case ServerGameState.EndRound:
            // Check if minimum # of clients to start a round exist
            const currentUsers = roomRef.state.networkedUsers.size;
            let minReqPlayersToStartRound = Number(roomOptions["minReqPlayers"] || 2);
            if(currentUsers < minReqPlayersToStartRound) {
                // Set room general message saying we're waiting for enough players to join the room
                roomRef.state.attributes.set(GeneralMessage, `Waiting for more players to join - (${currentUsers}/${minReqPlayersToStartRound})`);
                return;
            }

            // Now that we have enough players to start a round
            // check if all the users are ready to receive targets
            playersReady = checkIfUsersReady(roomRef.state.networkedUsers);

            // Return out if not all of the players are ready yet.
            if(playersReady == false) return;

            // Time to send targets to the clients
            moveToState(roomRef, ServerGameState.SendTargets);
            break;
        case ServerGameState.SendTargets:
            // Check if all the users are ready to proceed after receiving targets
            playersReady = checkIfUsersReady(roomRef.state.networkedUsers);

            // Return out if not all of the players are ready yet.
            if(playersReady == false) return;

            // Lock the room to prevent any more players from joining until after this round has ended
            roomRef.lock();

            // Time to begin the round!
            moveToState(roomRef, ServerGameState.BeginRound);
            
            break;
    }
}

function getRandomIntInclusive(min: number, max: number) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min + 1)) + min; //The maximum is inclusive and the minimum is inclusive 
}

/**
 * The logic run when the server is in the SendTargets state
 * @param {*} roomRef Reference to the room
 * @param {*} deltaTime Server delta time in seconds
 */
let sendTargetsLogic = function (roomRef: ShootingGalleryRoom, deltaTime: number) {

    // Set users "readyState" attribute to "waiting"
    // We're waiting for all clients to get the target data and prepare for the round to begin
    // before signalling they're ready to proceed.
    setUsersAttribute(roomRef, ClientReadyState, "waiting");

    // How many targets are in this round? Upper limit set to be 10 targets for every connected user, max 100
    const max = Math.min(roomRef.state.networkedUsers.size * 10, 100);
    const randomNumTargets = getRandomIntInclusive(10, max);

    // Get a random line up of targets
    const targets = getTargetSet(roomRef, randomNumTargets);

    // Send the targets to all connected clients
    roomRef.broadcast("newTargetLineUp", { targets });

    // Time to wait for all clients to tell us when they're ready with their targets
    moveToState(roomRef, ServerGameState.Waiting);
}

/**
 * The logic run when the server is in the BeginRound state
 * @param {*} roomRef Reference to the room
 * @param {*} deltaTime Server delta time in seconds
 */
let beginRoundLogic = function (roomRef: ShootingGalleryRoom, deltaTime: number) {
    
    let roomState = roomRef.state.attributes.get("CurrentCountDownState");
    if(roomState == null) {
        roomState = CountDownState.Enter;
        roomRef.state.attributes.set("CurrentCountDownState",roomState);
    }

    switch (roomState) {
        // Beginning a new round
        case CountDownState.Enter:
            
            // Reset the count down message attribute
            setRoomAttribute(roomRef, BeginRoundCountDown, "");

            // Broadcast to the clients that a round has begun
            roomRef.broadcast("beginRoundCountDown", {});

            // Reset count down helper value
            roomRef.state.attributes.set("currCountDown","0");

            // Move to the GetReady state of the count down
            roomState = CountDownState.GetReady;
            break;
        case CountDownState.GetReady:

            // Begin with "Get Ready!"
            // Set the count down message attribute
            setRoomAttribute(roomRef, BeginRoundCountDown, "Get Ready!");
            
            // Show the "Get Ready!" message for 3 seconds
            var currCountDown = Number(roomRef.state.attributes.get("currCountDown"));
            if(currCountDown < 3){
                currCountDown += deltaTime;
                roomRef.state.attributes.set("currCountDown",currCountDown.toString());
                return;
            }

            // Move to the CountDown state of the count down
            roomState = CountDownState.CountDown;

            roomRef.state.attributes.set("currCountDown",CountDownTime.toString());

            break;
        case CountDownState.CountDown:
            // Update Count Down value
            var currCountDown = Number(roomRef.state.attributes.get("currCountDown"));
            // Update count down message attribute
            setRoomAttribute(roomRef, BeginRoundCountDown, Math.ceil(currCountDown).toString());

            if (currCountDown >= 0) {
                currCountDown -= deltaTime;
                roomRef.state.attributes.set("currCountDown",currCountDown.toString());
                return;
            }

            // Tell all clients that round has begun!
            roomRef.broadcast("beginRound", {});

            // Move to the Simulation state
            moveToState(roomRef, ServerGameState.SimulateRound);

            // Clear user's ready state for round begin
            setUsersAttribute(roomRef, ClientReadyState, "waiting");

            // Reset Current Count Down state for next round
            roomState = CountDownState.Enter;
            break;
    }
    //Save room state
    roomRef.state.attributes.set("CurrentCountDownState",roomState);    
}

/**
 * The logic run when the server is in the SimulateRound state
 * @param {*} roomRef Reference to the room
 * @param {*} deltaTime Server delta time in seconds
 */
let simulateRoundLogic = function (roomRef: ShootingGalleryRoom, deltaTime: number) {
    // Check if any targets remain in the currentActiveTargets collection
    if (roomRef.currentActiveTargets && roomRef.currentActiveTargets.size > 0) {
        return;
    }

    // No more active targets
    // Double check that all targets have been claimed
    // Log an error if a target has not been claimed
    for(let target of Array.from(roomRef.currentTargetSet.values())){
        let targetObj: TargetObject = target as TargetObject;
        if(targetObj.claimed == false){
            logger.error(`No more active targets but target ${targetObj.uid} has not been claimed?`);
        }
    }

    // Move to the EndRound state
    moveToState(roomRef, ServerGameState.EndRound);
}

/**
 * The logic run when the server is in the EndRound state
 * @param {*} roomRef Reference to the room
 * @param {*} deltaTime Server delta time in seconds
 */
let endRoundLogic = function (roomRef: ShootingGalleryRoom, deltaTime: number) {

    // Get the winner of the round that just ended
    const winner = getRoundWinner(roomRef);
    // Let all clients know that the round has ended, sending the winner object to them
    roomRef.broadcast("onRoundEnd", { winner });

    // Reset the server state for a new round
    resetForNewRound(roomRef);

    // Move to Waiting state, waiting for all players to "ready up" for another round of play
    moveToState(roomRef, ServerGameState.Waiting);
}
//====================================== END GAME STATE LOGIC

// Room accessed functions
//======================================
/**
 * Initialize the Shooting Gallery logic
 * @param {*} roomRef Reference to the room
 * @param {*} options Options of the room from the client when it was created
 */
exports.InitializeLogic = function (roomRef: ShootingGalleryRoom, options: any) {

    roomOptions = options;

    
    roomRef.currentTargetSet = new Map();
    /** Collection of targets remaining to be claimed by entities. */
    roomRef.currentActiveTargets = new Map();
    /** Collection for tracking the game scores for a round. */
    roomRef.gameScores = new Map();

    // Set initial game state to waiting for all clients to be ready
    setRoomAttribute(roomRef, CurrentState, ServerGameState.Waiting)
    setRoomAttribute(roomRef, LastState, ServerGameState.None);
}

/**
 * Run Game Loop Logic
 * @param {*} roomRef Reference to the room
 * @param {*} deltaTime Server delta time in milliseconds
 */
exports.ProcessLogic = function (roomRef: ShootingGalleryRoom, deltaTime: number) {
    
     gameLoop(roomRef, deltaTime/1000); // convert deltaTime from ms to seconds
}

/**
 * Processes requests from a client to run custom methods
 * @param {*} roomRef Reference to the room
 * @param {*} client Reference to the client the request came from
 * @param {*} request Request object holding any data from the client
 */ 
exports.ProcessMethod = function (roomRef: ShootingGalleryRoom, client: Client, request: any) {
    
    // Check for and run the method if it exists
    if (request.method in customMethods && typeof customMethods[request.method] === "function") {
        customMethods[request.method](roomRef, client, request);
    } else {
        throw "No Method: " + request.method + " found";
        return; 
    }
}

/**
 * Process report of a user leaving. If we were previously locked due to a game starting and didn't
 * unlock at the end because the room was full, we'll need to unlock now
 */ 
 exports.ProcessUserLeft = function (roomRef: ShootingGalleryRoom) {
    if(roomRef.locked)
    {
        switch(getGameState(roomRef, CurrentState)){
        case ServerGameState.Waiting:
            unlockIfAble(roomRef);
            break;
        case ServerGameState.SendTargets:
        case ServerGameState.BeginRound:
        case ServerGameState.SimulateRound:
        case ServerGameState.EndRound:
            logger.silly(`Will not unlock the room, Game State - ${getGameState(roomRef, CurrentState)}`);
            break;
        }
    }
 }
//====================================== END Room accessed functions