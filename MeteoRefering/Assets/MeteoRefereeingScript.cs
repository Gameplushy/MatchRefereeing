﻿using KeepCoding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RNG = UnityEngine.Random;
using System.Linq;

public class MeteoRefereeingScript : ModuleScript {

	public GameObject arkGO;
	public KMSelectable ark;
	public PlanetWrapper[] planets;
	public Sprite[] planetImages;

	private PlanetNames[][] planetsUsed;
	private PlanetNames[] planetList;
	public GameObject[] stagesPlanets;
	private int stage = 0;
	private int levelIndex = 0;
	private int[] levelOrder;

	private bool sonarInputPossible = true;
	private bool planetInputPossible = true;

	public Texture[] spaceCraftAnimSprites;
	public Texture[] BGs;
	public MeshRenderer bgMat;

	private int GetPlanetArrayOffset() { return stage * 3 - (stage == 0 ? 0 : 1); }

	private static readonly Dictionary<PlanetNames, float[]> sfxTime = new Dictionary<PlanetNames, float[]>()
	{
			{PlanetNames.Anasaze,new float[] {1.5f,4.8f}},
			{PlanetNames.Bavoom,new float[] {5.4f,4.3f}},
			{PlanetNames.Boggob,new float[] {1.9f,1.3f}},
			{PlanetNames.Brabbit,new float[] {2.3f,2.3f}},
			{PlanetNames.Cavious,new float[] {4.3f,6.3f}},
			{PlanetNames.Dawndus,new float[] {3.9f,3.9f}},
			{PlanetNames.Firim,new float[] {5.6f,5.3f}},
			{PlanetNames.Florias,new float[] {4.6f,5.5f}},
			{PlanetNames.Forte,new float[] {3.2f,3.9f}},
			{PlanetNames.Freaze,new float[] {3.9f,3.7f}},
			{PlanetNames.Geolyte,new float[] {5.2f,3.8f}},
			{PlanetNames.Gigagush,new float[] {2.4f,4.9f}},
			{PlanetNames.Globin,new float[] {2.9f,4.1f}},
			{PlanetNames.Grannest,new float[] {3.7f,5f}},
			{PlanetNames.Gravitas,new float[] {5.3f,4.9f}},
			{PlanetNames.Hevendor,new float[] {5f,5.3f}},
			{PlanetNames.Hotted,new float[] {6.2f,6.2f}},
			{PlanetNames.Jeljel,new float[] {3.9f,6.4f}},
			{PlanetNames.Lastar,new float[] {5.3f,4.3f}},
			{PlanetNames.Layazero,new float[] {3.6f,4.3f}},
			{PlanetNames.LunaLuna,new float[] {5.3f,6.2f}},
			{PlanetNames.Megadom,new float[] {3.7f,3.7f}},
			{PlanetNames.Mekks,new float[] {5.3f,6.8f}},
			{PlanetNames.Meteo,new float[] {6.2f,4.9f}},
			{PlanetNames.Oleana,new float[] {3f,2.7f}},
			{PlanetNames.Starrii,new float[] {5.6f,6.2f}},
			{PlanetNames.Suburbion,new float[] {4.5f,4.8f}},
			{PlanetNames.Thirnova,new float[] {3.8f,7.3f}},
			{PlanetNames.Vubble,new float[] {6f,5.2f}},
			{PlanetNames.Wiral,new float[] {2.3f,1.8f}},
			{PlanetNames.Wuud,new float[] {4.3f,4.6f}},
			{PlanetNames.Yooj,new float[] {5.6f,5f}}
	};

	// Use this for initialization
	void Start () {
		
		planetList = (PlanetNames[])Enum.GetValues(typeof(PlanetNames)).Shuffle();
		planets.Select(p=>p.selectable).Assign(onInteract: Annihilate);
		stagesPlanets.ForEach(sp => sp.SetActive(false));
		ark.Assign(onInteract:()=>{ if(sonarInputPossible) StartCoroutine(Sonar()); });
		planetsUsed = new PlanetNames[3][];
		int planetIndex=0;
		for(int i = 0; i < 3; i++)
        {
			planetsUsed[i] = new PlanetNames[i + 2];
			for (int j = 0; j < i + 2; j++) {
				planetsUsed[i][j] = planetList[planetIndex];
				planets[planetIndex++].sr.sprite = planetImages[(int)planetsUsed[i][j]];
			}
			
        }
		StartStage();
	}

    private IEnumerator Sonar()
    {
		PlanetNames[] usedPlanets;// = planetsUsed[stage];
        if (RNG.Range(0, 4) == 3)
        {
			Log("Sonar is faulty, you're going to hear a different match !");
			do
				usedPlanets = planetList.Shuffle().Take(planetsUsed.Length).ToArray();
			while (usedPlanets.All(p => planetsUsed[stage].Contains(p)));
        }
        else
        {
			Log("Playing actual match...");
			usedPlanets = planetsUsed[stage];
        }
		float[] tmp = usedPlanets.Select(pu => { float[] res; sfxTime.TryGetValue(pu, out res); return res[pu == usedPlanets[levelOrder.Last()] ? 1 : 0]; }).ToArray();
		Log(tmp);
		float[] timeTable = new float[stage + 2];
		for(int i=0;i<timeTable.Length;i++) timeTable[i] = tmp[levelOrder[i]];
		Log(timeTable);
		float[] delays = new float[stage];
		for (int i = 0; i < delays.Length; i++) delays[i] = RNG.Range(2f, 5f);
		Log(delays);
		for(int i=0;i<timeTable.Length;i++)
        {
			if (i == 0) continue;
			timeTable[i] += delays.Take(Math.Min(i - 1,delays.Length-1)).Sum();
        }
		Log(timeTable);
		StartCoroutine(BlockSonar(timeTable.Max()));
        for(int i = 0; i < stage + 2; i++)
        {
			if (i != stage + 1 && i!=0) yield return new WaitForSecondsRealtime(delays[i-1]);
			Log(usedPlanets[levelOrder[i]].ToString());
			PlaySound(usedPlanets[levelOrder[i]].ToString()+(i == stage+1?"V":"A"));
			
        }
    }

    private IEnumerator BlockSonar(float v)
    {
		sonarInputPossible = false;
		yield return new WaitForSecondsRealtime(v);
		sonarInputPossible = true;
    }

    private void StartStage()
    {
		stagesPlanets[stage].SetActive(true);
		float[] offsets = Enumerable.Range(0, 4).Select(x => (float)x / 4).ToArray().Shuffle();
		int[] starts = new int[] { 0, 2, 5 };
		for (int i = starts[stage]; i < starts[stage]+stage+2; i++) planets[i].animator.SetFloat("offset", offsets[i-starts[stage]]);
		Log("Stage {0} : Opponents are {1}.", stage + 1, planetsUsed[stage].Join(","));
		levelOrder = Enumerable.Range(0, stage + 2).ToArray().Shuffle();
		Log(levelOrder);
		Log("Order from last to first is : {0}.", levelOrder.Select(x => planetsUsed[stage][x]));
	}

    private void Annihilate(int obj)
    {
		Log(levelOrder[levelIndex]);
		if (obj-GetPlanetArrayOffset() == levelOrder[levelIndex])
		{
			PlaySound("annihilate");
			planets[obj].animator.enabled = false;
			planets[obj].material.SetFloat("_GrayscaleAmount", 1);
            if (++levelIndex == stage + 1)
            {
				levelIndex = 0;
				if (++stage == 3) Solve();
				else StartCoroutine(NextStage());
            }
		}
		else Strike();
    }

    private IEnumerator NextStage()
    {
		planetInputPossible = false;
		yield return new WaitForSeconds(2f);
		arkGO.SetActive(false);
		stagesPlanets.ForEach(sp => sp.SetActive(false));
		for(int i = 0; i < spaceCraftAnimSprites.Length; i++)
        {
			bgMat.material.mainTexture = spaceCraftAnimSprites[i];
			if (i == 20) PlaySound("ignite");
			yield return new WaitForSecondsRealtime((float)1 / 60);
        }
		bgMat.material.mainTexture = BGs[stage];
		StartStage();
		arkGO.SetActive(true);
		planetInputPossible = true;
	}
#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"[!{0} sonar] activates your sonar (it'll wait until you'll be able to activate it). [!{0} annihilate #] annihilates the #th planet, counting from the top. You can annihilate multiple planets in the same command.";
#pragma warning restore 414
	private IEnumerator ProcessTwitchCommand(string command)
    {
		string[] commands = command.Split(" ");
        if (commands[0].Equals("sonar", StringComparison.InvariantCultureIgnoreCase))
        {
			yield return null;
			yield return new WaitUntil(() => sonarInputPossible);
			ark.OnInteract();
        }
		else if (commands[0].Equals("annihilate", StringComparison.InvariantCultureIgnoreCase) && commands.Skip(1).All(n=>Enumerable.Range(1,stage+2).Select(i=>i.ToString()).Contains(n)) && commands.Length - 1 > 0 && commands.Length - 1 <= stage + 1 - levelIndex)
        {
			
			yield return null;
			foreach(int index in commands.Skip(1).Select(n => int.Parse(n)-1)){
				yield return new WaitUntil(() => planetInputPossible);
				planets[index + GetPlanetArrayOffset()].selectable.OnInteract();
				yield return new WaitForSecondsRealtime(.3f);
            }
        }
    }
	private IEnumerator TwitchHandleForcedSolve()
    {
        while (!IsSolved)
        {
			yield return new WaitUntil(() => planetInputPossible);
			planets[levelOrder[levelIndex] + GetPlanetArrayOffset()].selectable.OnInteract();
			yield return new WaitForSecondsRealtime(.3f);
        }
    }
}
