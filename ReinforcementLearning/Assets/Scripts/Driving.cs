using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;




public class Driving: MonoBehaviour {

	[Header("Genetic Algorithm")]
	[SerializeField] float mutationRate = 0.01f;
	[SerializeField] float timeScale = 1f;
	[SerializeField] int numberOfAgents = 50;
	[SerializeField] int DNAsize = 6;
	[SerializeField] float timeEachGeneration = 30f;
	[SerializeField] float wrongWayMultipler = 0.05f;
	[SerializeField] int elitism = 1;

    [Header("Exit Conditions")]
    [SerializeField] int runsNeeded = 1;
    [SerializeField] int generations = 30;

    [Header("Data Writing")]
	public CSVWriter CSVWriter;

    [SerializeField] GameObject CheckPointHolder;
	
	[Header("Text Labels")]
	[SerializeField] TextMeshProUGUI bestFitnessText;
	[SerializeField] TextMeshProUGUI numGenText;
	[SerializeField] TextMeshProUGUI bestLapText;

	[Header("Button Text")]
	[SerializeField] TextMeshProUGUI buttonText;


	[Header("Agent Prefab")]
	[SerializeField] GameObject agentPrefab;

	private GeneticAglorithm<float> ga;		// Genetic Aglorithm with each Gene being a float
	private AgentManager agentManager;		// Manager for handling a jumping agents we need to make
	private System.Random random;			// Random for the RNG
	private bool running = false;			// Flag for if the GA is to run
	private float bestJump = 0;             // Store for the highest jump so far - Used for Mutation

	private float timer = 0f;
	float bestLap =0f;
	int consecutiveGoodRuns = 0;

	// Use this for initialization
	void Start () {

		// create the csv writer class
		//CSVWriter = new CSVWriter();
		// Create the Random class
		random = new System.Random();
        Time.timeScale = timeScale;
		
        // Create our Agent Manager and give it the agent prefab
        agentManager = new AgentManager(agentPrefab, this.transform, numberOfAgents, CheckPointHolder.transform);

		// Create genetic algorithm class
		ga = new GeneticAglorithm<float>(agentManager.agents.Count, DNAsize, random, GetRandomGene, FitnessFunction, mutationRate:mutationRate, elitism);

		// apply genes to values of cars
		for(int i = 0; i < numberOfAgents; i++)
		{
			AIController ai = agentManager.agents[i].GetComponent<AIController>();
			if(ai != null )
			{
				ai.ApplyGenes(ga.Population[i]);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (running)
		{
            // Update time scale based on Editor value - do this every frame so we capture changes instantly
            Time.timeScale = timeScale;
			UpdateText();
			timer += Time.deltaTime;

			if (!agentManager.AnyAgentAlive() || timer > timeEachGeneration || Input.GetKeyDown(KeyCode.Space))
			{
				Time.timeScale = 1f;

				ga.NewGeneration();

				GetBestLapTime();
				CollectData();
				
				if (SolutionReached())
				{
                    //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    Application.Quit();
                    UnityEditor.EditorApplication.isPlaying = false;
                }/*
				if(ga.Generation > generations)
				{
                    //Application.Quit();
                    //UnityEditor.EditorApplication.isPlaying = false;
                }*/
				timer = 0f;

				DeleteAllAgents();
				agentManager.agents.Clear();
				agentManager.CreateAgents(agentPrefab, this.transform, numberOfAgents, CheckPointHolder.transform);

				// apply genes to values of cars
				for (int i = 0; i < numberOfAgents; i++)
				{
					AIController ai = agentManager.agents[i].GetComponent<AIController>();
					if (ai != null)
					{
						ai.ApplyGenes(ga.Population[i]);
					}
				}

	
			}
		}
		else
		{
			Time.timeScale = 0;
		}

	}

	// all of our genes are float values from 0-1, they act as a percentage of the max valeu
	private float GetRandomGene()
	{
		// Generate a new jump value based on the current best jump (the higher the best jump, the greater the next random gene)
		/*float next = (float)random.NextDouble();
		return (next*value);*/

		
		return (float)random.NextDouble();
	}

	private float FitnessFunction(int index)
	{
        float score = 0;
		// get ai controller for the agent
		AIController ai = agentManager.agents[index].GetComponent<AIController>();

		// for a lap time goal of 10 
		float defaultTimePerCheckpoint = 10f / (float)CheckPointHolder.transform.childCount;

        // increase fit based on checkpoints passed
        float checkPoints = (float)ai.CheckPointsPassedInOrder / (float)CheckPointHolder.transform.childCount;

		score += checkPoints;
		

		// give a bonus for completing the track
		if (ai.CheckPointsPassedInOrder >= CheckPointHolder.transform.childCount)
		{
			score += 15f;
            // give a bonus based on the time taken to complete a lap 

			//score += 10 / ai.timeAlive * 10f;
			score += MathF.Pow(10, 10 / ai.timeAlive);
        }
		else
		{
			// if it didn't complete the track give a small bonus based on time survived
			// if it passed checkpoints
			if (ai.CheckPointsPassedInOrder > 0)
			{
				score += ai.timeAlive / 100;

            }
        }

        // if ai passed the checkpoints in the wrong order then penalize the fitness quite heavily
        if (ai.wentTheWrongWay)
            score *= wrongWayMultipler;
        return score;
	}

	public void onButtonClick()
	{
		 //Pause and Start the algorithm
		if(running)
		{
			running = false;
			if(buttonText)
			{
				buttonText.text = "Start";
			}
		}
		else
		{
			running = true;
			if(buttonText)
			{
				buttonText.text = "Pause";
			}
		}
	}

	private void UpdateText()
	{

		if(bestFitnessText)
		{
			bestFitnessText.text = ga.BestFitness.ToString();
		}

		if(numGenText)
		{
			numGenText.text = ga.Generation.ToString();
		}
		if(bestLapText)
		{
			bestLapText.text = bestLap.ToString();
		}

	}


    public void DeleteAllAgents()
    {
        foreach (GameObject agent in agentManager.agents)
        {
            AIController ai = agent.GetComponent<AIController>();
			Destroy(ai.sphereRB.gameObject);
            Destroy(agent.gameObject);
        }
    }

	public bool SolutionReached()
	{
		if (AgentsThatCompletedTheTrack() > numberOfAgents * 0.75)
			consecutiveGoodRuns++;
		else
			consecutiveGoodRuns = 0;

		if (consecutiveGoodRuns >= runsNeeded)
			return true;
		else
			return false;
	}


    //--------- DATA COLLECTION


    public void GetBestLapTime()
	{
		foreach(GameObject agent in agentManager.agents)
		{
            AIController ai = agent.GetComponent<AIController>();
            if (ai.CheckPointsPassedInOrder >= CheckPointHolder.transform.childCount)
            {
				if (ai.timeAlive < bestLap || bestLap == 0f)
				{
					bestLap = ai.timeAlive;
				}
            }
        }
	}

	public float CurrentGenerationBestLap()
	{
		float bestLap = 0f;
        foreach (GameObject agent in agentManager.agents)
        {
            AIController ai = agent.GetComponent<AIController>();
            if (ai.CheckPointsPassedInOrder >= CheckPointHolder.transform.childCount)
            {
                if (ai.timeAlive < bestLap || bestLap == 0f)
                {
                    bestLap = ai.timeAlive;
                }
            }
        }

		return bestLap;
    }



	public void CollectData()
	{
		float averageFitness = ga.CalculateAverageFitness();
		float bestFit = ga.BestFitness;
		int gen = ga.Generation;
		int goodCars = AgentsThatCompletedTheTrack();

		gen--;
		CSVWriter.DataToWrite data = new CSVWriter.DataToWrite();
		data.bestFitness = bestFit;
		data.goodCars = goodCars;
		data.generation = gen;
		data.averageFitness = averageFitness;
		data.bestLap = CurrentGenerationBestLap();
		CSVWriter.AllData.Add(data);
	}

    public int AgentsThatCompletedTheTrack()
    {
        int total = 0;
        foreach (GameObject agent in agentManager.agents)
        {
            AIController ai = agent.GetComponent<AIController>();
			if (ai.CheckPointsPassedInOrder > CheckPointHolder.transform.childCount)
				total++;
        }

        return total;
    }

    public void OnApplicationQuit()
    {
		CSVWriter.WriteCSV();
    }

}


