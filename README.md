# Q-State-Machine
Implementation of Evolving State Machine Based on Q-Learning

##Usage
This AI must be run in the editor for now until I've tested that the executable version works.
Open "**Assets/Seed Menu.unity**". Run the scene, enter the seed and then the experiment will begin with the given seed value for the random number generator. Using the seed, you can repeat an experiment run.
The scene camera can only be controlled when the **RMB** is held down. The controls are:
* **Move the mouse** to change where the camera is looking.
* **WASD** to move forward, right, left, backwards and right, respectively.
* **E** to move up, **F** to move down
* Hold **MMB** or **space** while moving the mouse to orbit the camera around the centre of the scene
* Pressing **J** resets the camera to where it was facing when the experiment started.

## The AI
This AI is only referred to as the Q-State-Machine since it was inspired by Q-Learning. However, with Q-Learning we have the AI state as nodes and the actions as transitions (and the AI tries to learn the transition that yields the greatest reward, for every state). With this AI, the actions are the nodes and the states are transitions. The AI seeks to evolve it's transitions and nodes to maximise the reward the AI gets over time. The AI evolves by:
* Adding, removing, or swapping the conditions of a transition (state constraint), or shifting the thresholds
* Adding, removing, or swapping the conditions of a node (actions)

This AI is a strategic AI - it does not focus on how to find a path, for example. It's basic actions (moving, shooting etc.) are defined by heuristics. It learns when to shoot, move etc. and can chain these actions together e.g. "If I can see a enemy, shoot it. If I can still see it after that, retreat."

Whatever transition whose conditions best match the state of the agent is the one that is chosen. If the AI state does not meet any transmition's requirements for the AI state, a random node (set of actions) is transitioned to.

To cut down learning time, I initialized the AI with a heuristic state machine and pitted a evolving version of the AI against the heuristic version of the AI. So far this AI is at least usable for fast-paced game AI.

## Testing
The AI test involves two AI agents, Red and Blue, fighting in an arena. They both start with a heuristic AI state machine but the Red AI has a static state machine while the Blue AI has 10 variations of the state machine it tries every round. At the end of the round the worst-performing state machine is replaced by a modified version of the best-performing state machine. Note that every time a node is mutated a random new node is added into the graph. This new node is connected to at least one of the other nodes.
