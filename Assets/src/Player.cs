//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

public enum Resource {FOOD=0, WOOD=1, GOLD=2, STONE=3};

public class Player
{
	public Color color;
	public int number;
	public List<Unit> units = new List<Unit>(); // units add themselves
	public List<Building> buildings = new List<Building>(); // buildings add themselves

	public HashSet<Building> tradeWith = new HashSet<Building>();

	public bool isHuman = false;
	public bool isNeutral = false;
	public Dictionary<Resource, float> resources;
	public Dictionary<Resource, UnityEngine.UI.Text> resourceText;

	public Player (int pnum)
	{
		number = pnum;
		switch (number) {
		case 0:
			color = Color.gray;
			isNeutral = true;
			break;
		case 1:
			color = Color.blue;
			isHuman = true;
			break;
		case 2:
			color = Color.green;
			break;
		case 3:
			color = Color.red;
			break;
		case 4:
			color = Color.black;
			break;
		}
		resources = new Dictionary<Resource, float>();
		foreach (Resource res in Enum.GetValues(typeof(Resource))) {
			resources[res] = 0f;
		}
		collect (Resource.FOOD, 100f);
		collect (Resource.GOLD, 50f);
	}
	
	public void collect(Resource res, float amount) {
		resources[res] += amount;
		amountChanged(res);
	}

	private void amountChanged(Resource res) {
		if (resourceText != null && resourceText.ContainsKey(res)) {
			resourceText[res].text = resources[res].ToString();
		}
	}
	
	public bool has(Dictionary<Resource, int> costDict) {
		foreach (KeyValuePair<Resource, int> kvp in costDict) {
			if (kvp.Value > resources[kvp.Key]) {
				return false;
			}
		}
		return true;
	}
	
	public bool has(Resource res, float amount) {
		return resources[res] >= amount;
	}

	public bool spend(Dictionary<Resource, int> costDict) {
		if (has(costDict)) {
			foreach (KeyValuePair<Resource, int> kvp in costDict) {
				spend (kvp.Key, kvp.Value);
			}
			return true;
		} else {
			return false;
		}
	}

	public bool spend(Resource res, float amount) {
		if (has(res, amount)) {
			resources[res] -= amount;
			amountChanged(res);
			return true;
		} else {
			return false;
		}
	}
	
	public void toggleTrading(Building building) {
		if (tradeWith.Contains(building)) {
			tradeWith.Remove(building);
		} else {
			tradeWith.Add(building);
		}
		building.setOwner(this);
	}

	public HashSet<Building> tradeBuildings() {
		return tradeWith;
	}

	public void setGUI(UnityEngine.UI.Text[] resText) {
		resourceText = new Dictionary<Resource, UnityEngine.UI.Text>();
		resourceText[Resource.FOOD] = resText[0];
		resourceText[Resource.GOLD] = resText[1];
		amountChanged(Resource.FOOD);
		amountChanged(Resource.GOLD);
	}
}
