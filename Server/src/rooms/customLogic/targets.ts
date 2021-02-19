import { generateId } from "colyseus";

export default {
    targets: 
    [
        {
            id: 1,
            name: "Target 1",
            value: 5
        },
        {
            id: 2,
            name: "Target 2",
            value: 10
        },
        {
            id: 3,
            name: "Target 3",
            value: 15
        },
        {
            id: 4,
            name: "Target 4",
            value: 30
        },
        {
            id: 5,
            name: "Target 5",
            value: 50
        },
        {
            id: 6,
            name: "Target 6",
            value: 100
        }
    ],

    getRandomTarget : function(numRows: number) {

        let randomIdx = Math.ceil(Math.random() * 6);

        // logger.silly("Targets List:");
        // console.log(this.targets);

        if(randomIdx < 0)
            randomIdx = 0;
        else if(randomIdx > this.targets.length - 1)
            randomIdx = this.targets.length - 1;

        let target: any = {...this.targets[randomIdx]};

        // assign a unique ID to the target
        target.uid = generateId();
        // A target is considered to be claimed once a player has scored/shot it
        target.claimed = false;
        // Determine which row this target will go into
        target.row = Math.ceil(Math.random() * numRows);

        return target;
    },

    // TODO: allow specification to limit number of a type of target so we don't just get a bunch of high point targets
    getRandomTargetLineUp: function(numOfTargets: number, numRows: number) {
        const targetLineUp = [];

        for(let i = 0; i < numOfTargets; i++) {
            targetLineUp.push(this.getRandomTarget(numRows));
        }

        return targetLineUp;
    }
}
