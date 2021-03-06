﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityBaseCode.Actions;
using UnityBaseCode.Statuses;
using UnityBaseCode.Steering;

public enum BuildingType {BASE=0, COLONY=1};

public class Building : MonoBehaviour, Attackable, Clickable, ObjectWithPosition {
	
	[HideInInspector]
	public bool dead {get; set;}
	[HideInInspector]
	public Player owner;

	private float amount = 0f;
	public float maxAmount = 50f;
	[HideInInspector]
	public Resource resource = Resource.FOOD;
	private float productionRate = 1f;

	// attackable interface
	public float radius {get; set;}
	// clickable interface
	public ClickRegion clickArea = new CircleRegion(0.7f);

	public float influenceRadius;
	
	private int maxHealth = 10;
	private int health;
	private float hpPercent = 1f;
	
	[HideInInspector]
	public Vector2 tilePos;
	[HideInInspector]
	public Vector3 gamePos;
	public BuildingType type;
	private Transform dock;

	private List<UnitType> trainableUnitTypes = new List<UnitType>();

	private SpriteRenderer tradeWith;
	private SpriteRenderer influence;

	private ParticleSystem fireEmitter;

	public void init(Vector2 tileCoordinate, Vector2 dockTileCoordinate, BuildingType buildingType) {
		type = buildingType;
		tilePos = tileCoordinate;
		Map map = Scene.get().map;
		gamePos = map.mapToGame(tileCoordinate);
		transform.position = new Vector3(gamePos.x, gamePos.y, 0f);
		
		Vector2 dockSide = dockTileCoordinate - tileCoordinate;
		float angle = Mathf.Rad2Deg * Mathf.Atan2(dockSide.y, dockSide.x);
		transform.FindChild("dockRotation").localEulerAngles = new Vector3(0f,0f,angle);
		dock = transform.FindChild("dockRotation/dotted square");
		
		trainableUnitTypes.Add(UnitType.MERCHANT);
		trainableUnitTypes.Add(UnitType.GALLEY);
		trainableUnitTypes.Add(UnitType.LONGBOAT);
		trainableUnitTypes.Add(UnitType.TRADER);
		
		amount = 0f;
		health = maxHealth;
		radius = 0.25f;
		dead = false;
		
		Transform influenceObj = transform.FindChild("influence");
		influence = influenceObj.GetComponent<SpriteRenderer>();
		tradeWith = transform.FindChild("tradeWith").GetComponent<SpriteRenderer>();
		
		switch(buildingType) {
		case BuildingType.BASE:
			influenceRadius = 3f;
			break;
		case BuildingType.COLONY:
		default:
			influenceRadius = 2.5f;
			break;
		}
		// the buildign itself has a scale
		float sz = (2f * influenceRadius / influence.sprite.bounds.size.x) * (1f/transform.localScale.x);
		influenceObj.localScale = new Vector3(sz, sz, 1f);
		tradeWith.enabled = false;
		
		fireEmitter = transform.FindChild("Fire").GetComponent<ParticleSystem>();
		fireEmitter.enableEmission = false;
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (type != BuildingType.BASE) {
			amount = Mathf.Min(amount + productionRate * Time.deltaTime, maxAmount);
			float sz = 0.3f + 1.2f * amount / maxAmount;
			transform.FindChild("resource").localScale = new Vector3(sz,sz,1f);
		}
	}

	public float collect(float capacity) {
		float amountTaken = Mathf.Min(capacity, amount); 
		amount -= amountTaken;
		return amountTaken;
	}

	public float expectedProfit(float arrivalDelay) {
		// TODO: take other traders into account?
		float expectedAmount = Mathf.Min(amount + productionRate * arrivalDelay, maxAmount);
		return expectedAmount;
	}

	public void setOwner(Player p) {
		if (owner != null) {
			owner.buildings.Remove(this);
			if (type == BuildingType.BASE) {
				foreach (Unit u in owner.units) {
					u.dead = true;
				}
			}
		}
		owner = p;
		p.buildings.Add(this);
		transform.FindChild("teamColoredBuilding").GetComponent<Renderer>().material.SetColor("_TeamColor", owner.color);
		// The influence of a building is a circle around it in which friendly boats are healed.
		float influenceAlpha = p.isNeutral ? 0.15f : 0.25f;
		influence.color = new Color(p.color.r, p.color.g, p.color.b, influenceAlpha);
	}

	public void toggleTradeWith(Player p) {
		if (p.isHuman) {
			tradeWith.enabled = !tradeWith.enabled;
			tradeWith.color = p.color;
		}
	}

	// fits the "RadialMenu callback"
	public void trainUnit(int optionSelected) {
		if (optionSelected != -1) {
			UnitType unitType = trainableUnitTypes[optionSelected];
			var cost = UnitData.getCost(unitType);
			if (owner.spend(cost)) {
				Scene.get().spawnUnit(this, unitType);
			}
		}
	}

	public Vector2 getDock() {
		return dock.position;
	}

	public float getDockRadius() {
		return 0.6f;
	}

	/* 
	 * ObjectWithPosition
	 */

	public Vector2 getPosition() {
		return transform.position;
	}

	/* 
	 * ATTACKABLE 
	 */
	public void damage(Actor attacker, int amount) {
		if (attacker.playerNumber == owner.number) {
			return;
		}
		health -= amount;
		if (health <= 0) {
			health = maxHealth;
			setOwner(Scene.get().players[attacker.playerNumber]);
		}
		if (health < maxHealth) {
			fireEmitter.enableEmission = true;
			fireEmitter.startSize = 2f * (1f - health/maxHealth);
			fireEmitter.emissionRate = 50 * (1f - health/maxHealth);
		} else {
			fireEmitter.enableEmission = false;
		}
	}
	
	/* 
	 * CLICKABLE 
	 */
	public bool clickTest(int mouseButton, Vector2 worldMousePos) {
		// TODO: check a circle that is facing the camera, and at a custom world-z.
		return clickArea.Contains(worldMousePos - (Vector2) transform.position);
	}

	public void handleClick(int mouseButton) {
		if (type == BuildingType.COLONY) {
			Scene.get().players[Player.HUMAN_PLAYER].toggleTrading(this);
		}
	}

	public void handleDrag(int mouseButton, Vector2 offset, Vector2 relativeToClick) {}
	
	public void setHover(bool isHovering)
	{
		if (isHovering) {
			//transform.FindChild("ring").GetComponent<SpriteRenderer>().color = new Color(1f, 0.7f, 0.7f);
		} else {
			//transform.FindChild("ring").GetComponent<SpriteRenderer>().color = Color.white;
		}
	}

	public void handleMouseDown(int mouseButton, Vector2 mousePos) {
		if (type == BuildingType.BASE && owner.isHuman) {
			List<Sprite> icons = new List<Sprite>();
			foreach (UnitType uType in trainableUnitTypes) {
				icons.Add(UnitData.getIcon(uType));
			}
			GUIOverlay.get().createRadial(transform.position, icons, trainUnit);
		}
	}

	public void handleMouseUp(int mouseButton, Vector2 mousePos) {}

}
