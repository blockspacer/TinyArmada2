using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitGroup : IEnumerable<Unit>
{
	public Vector2 center;
	public float radius;
	public Color color;
	public int numUnits;
	private HashSet<Unit> units;

	private static Color[] colors = {
		new Color(1,0,0, 0.8f),
		new Color(0,1,0, 0.8f),
		new Color(0,0,1, 0.8f),
		new Color(0,1,1, 0.8f),
		new Color(1,0,1, 0.8f),
		new Color(1,1,0, 0.8f)
	};

	public UnitGroup ()
	{
		numUnits = 0;
		radius = 0;
		units = new HashSet<Unit>();
		color = colors[Random.Range(0, colors.Length)];
	}

	public bool isEmpty() {
		return units.Count == 0;
	}

	public void Add(Unit u) {
		units.Add(u);
	}
	
	public void Remove(Unit u) {
		units.Remove(u);
	}

	//http://stackoverflow.com/questions/8760322/troubles-implementing-ienumerablet

	// this is generic
	public IEnumerator<Unit> GetEnumerator() {
		return units.GetEnumerator();
	}

	// this is not generic
	IEnumerator IEnumerable.GetEnumerator() {
		return this.GetEnumerator();
	}

	public void update() {
		radius = 0.4f * Mathf.Sqrt(units.Count);
		
		center = new Vector2(0,0);
		foreach (Unit unit in units) {
			center += (Vector2) unit.transform.position;
		}
		numUnits = units.Count;
		center = center/numUnits;
		// each has radius 0.25 and area pi r 2
		// so n of them has total area n pi r 2 => scale radius by sqrt(n)
	}
}
