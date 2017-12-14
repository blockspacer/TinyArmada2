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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Resource {FOOD=0, WOOD=1, GOLD=2, STONE=3};

public class Player
{
	public Color color;
	public int number;
	public List<Unit> units = new List<Unit>(); // units add themselves
	public List<Building> buildings = new List<Building>(); // buildings add themselves
	public List<UnitGroup> unitgroups = new List<UnitGroup>(); // groups add themselves

	public bool isHuman = false;
	public bool isNeutral = false;
	public Dictionary<Resource, float> resources;
	public Dictionary<Resource, UnityEngine.UI.Text> resourceText;

	// the set of buildings you want to trade with
	public HashSet<Building> tradeWith = new HashSet<Building>();

	// optimal route from dock to each tradeWith 
	private Dictionary<Building, Path> tradeRoute = new Dictionary<Building, Path>();
	// list of units currently heading to a given building
	private Dictionary<Building, HashSet<Unit>> unitsTrading = new Dictionary<Building, HashSet<Unit>>();

	public Player (int pnum)
	{
		number = pnum;
		switch (number) {
		case 0:
			color = Color.gray;
			isNeutral = true;
			break;
		case 1:
			//Color.blue;
			color = new Color(0.72f, 0.78f, 1f);
			color = new Color(0.32f, 0.48f, 1f);
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
		collect (Resource.FOOD, 150f); // temporary fix for spawning units on top of each other resulting in NaN avoidance
		collect (Resource.GOLD, 50f);
	}
	
	public void collect(Resource res, float amount) {
		resources[res] += amount;
		amountChanged(res);
	}

	private void amountChanged(Resource res) {
		if (resourceText != null && resourceText.ContainsKey(res)) {
			resourceText[res].text = ((int) resources[res]).ToString();
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
			List<Unit> toStop = new List<Unit>(unitsTrading[building]);
			tradeWith.Remove(building);
			foreach (Unit u in toStop) {
				u.stop();
			}
		} else {
			tradeWith.Add(building);
			getTradeRoute(building); // force tradeRoute to be calculated
		}
		building.toggleTradeWith(this); // updates the visual
	}

	public HashSet<Building> tradeBuildings() {
		return tradeWith;
	}

	// Provides a reference to the resource text objects in the UI.
	public void setGUI(UnityEngine.UI.Text[] resText) {
		resourceText = new Dictionary<Resource, UnityEngine.UI.Text>();
		resourceText[Resource.FOOD] = resText[0];
		resourceText[Resource.GOLD] = resText[1];
		amountChanged(Resource.FOOD);
		amountChanged(Resource.GOLD);
	}

	public Building getBase() {
		return buildings[0];
	}
	
	public Path getTradeRoute(Building building) {
		if (!tradeRoute.ContainsKey(building)) {
			tradeRoute[building] = Pathing.findPath(getBase().getDock(), building.getDock(), 0.25f);
			unitsTrading[building] = new HashSet<Unit>();
		}
		return tradeRoute[building].copy();
	}

	public Path getReturnRoute(Building building) {
		return tradeRoute[building].reversedCopy();
	}

	public HashSet<Unit> getTraders(Building building) {
		return unitsTrading[building];
	}
	
	public void headingTo(Unit u, Building b) {
		unitsTrading[b].Add(u);
	}

	public void noLongerHeadingTo(Unit u, Building b) {
		unitsTrading[b].Remove(u);
	}
	
	//TODO: add scene state / unit list or whatever else is needed here later
	public void think() {
		if (isHuman) return;
		if (!isNeutral) {
			if (has (UnitData.getCost(UnitType.TRADER))) {
				//TODO: change trainunit to take unit type argument
				getBase().trainUnit(3); // UnitType.MERCHANT);
			}
		}
		tradeWithNClosest(Mathf.Min(8, 1 + units.Count));
	}

	public void tradeWithNClosest(int n) {
		// TODO: add/remove to tradewith if adding would yield higher profit than any existing building in tradewith
		while (tradeWith.Count < n) {
			Building closest = null;
			float routeLen = float.MaxValue;
			foreach (Building building in Scene.get().buildings) {
				if (tradeWith.Contains(building) || building.type != BuildingType.COLONY) continue;
				float distmin = (building.getDock() - getBase().getDock()).magnitude;
				if (distmin > routeLen) continue;
				Path p = getTradeRoute(building);
				if (p.length < routeLen) {
					routeLen = p.length;
					closest = building;
				}
			}
			// Edge case: fewer than n buildings exist.
			if (closest == null) {
				return;
			}
			toggleTrading(closest);
		}
	}
}
