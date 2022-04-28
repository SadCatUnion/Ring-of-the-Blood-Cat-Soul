```mermaid
stateDiagram-v2

[*] --> Locomotion
Locomotion --> InputAction : Input

%% OnUpdate: Rotate and Move
state Locomotion {
	direction LR
	
	[*] --> Idle
    Idle --> Walk
    Walk --> Idle
    Walk --> Run
    Run --> Walk
    Run --> Sprint
    Sprint --> Run
}

state InputAction {
	state if <<choice>>
	[*] --> if
	if --> Evade
}

```