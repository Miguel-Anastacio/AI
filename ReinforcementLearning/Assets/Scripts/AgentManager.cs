using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using Unity.VisualScripting;

public class AgentManager
{ 

	public int numberOfAgents;
	public List<GameObject> agents;

	public AgentManager(GameObject agentPrefab, Transform start, int amount, Transform CheckpointHolder)
	{
		CreateAgents(agentPrefab, start, amount, CheckpointHolder);
	}

	public bool AnyAgentAlive()
	{
		foreach(GameObject agent in agents)
		{
			if(agent.activeInHierarchy)
				return true;
		}

		return false;
	}
	
	public void CreateAgents(GameObject agentPrefab, Transform start, int amount, Transform CheckpointHolder)
	{
        agents = new List<GameObject>();
        numberOfAgents = amount;
        for (int i = 0; i < numberOfAgents; i++)
        {
            GameObject car = GameObject.Instantiate(agentPrefab, start);
            AIController ai = car.GetComponent<AIController>();
            ai.CheckPointHolder = CheckpointHolder;
            agents.Add(car);
        }
    }


}