using UnityEngine;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using AssemblyCSharp;

/// <summary>
/// Represents a agent that takes action based on it's state, where the relationship
/// between state and action taken is determined by the given QSensor
/// </summary>
public abstract class QAgent : MonoBehaviour {

	public string qFileName; // file describing initial agent

	//get the action(s) a agent should perform given it's current state
	public abstract List<string> GetAction (List<string> state, List<float> variables); // we use the list of strings as a return parameter as we want to facilitate concept-action-mapping

	//give the agent a reward instantly. The agent will add a reduced amount of the given reward value for the reward for it's given state/action pair
	public abstract bool RewardAgent (float reward); 

	//determines agent state and the actions taken in response to state
	public QSensor stateDetector;

	public abstract void Reset();

}
