﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapController : MonoBehaviour {

	[System.Serializable]
	public struct Coord 
	{
		public int x;
		public int y;
		
		public Coord(int _x, int _y) 
		{
			x = _x;
			y = _y;
		}
		
		public static bool operator ==(Coord c1, Coord c2)
		{
			return c1.x == c2.x && c1.y == c2.y;
		}
		
		public static bool operator !=(Coord c1, Coord c2)
		{
			return !(c1 == c2);
		}
		
	}

	List<Vector3> positions;
	List<Quaternion> rotations;
	List<Vector3> scales;
	List<GameObject> shrinkObjects;

	public GameObject tilePrefab;
	public int MapSizeX;
	public int MapSizeY;
	public float tileScale;
	public int tileTrapPercentage = 10;
	Transform[,] tileMap;

	void OnAwake()
	{
	
	}

	public void Start()
	{
		//GenerateMap();
		GameObject[] objects = GameObject.FindGameObjectsWithTag ("NetworkingObject");
		positions = new List<Vector3> ();
		rotations = new List<Quaternion> ();
		scales = new List<Vector3> ();
		shrinkObjects = new List<GameObject> ();
		                          
	
		for(int i = 0; i < objects.Length; ++i)
		{
			if(objects[i].GetComponent<ReshrikingEntity>())
			{
				positions.Add (objects[i].transform.position);
				rotations.Add (objects[i].transform.rotation);
				scales.Add ( objects[i].transform.localScale);
				shrinkObjects.Add (objects[i]);
			}
		}
	}

	public void Reset()
	{
		for (int i = 0; i < shrinkObjects.Count; ++i) {
			if(!shrinkObjects[i].GetActive())
				shrinkObjects[i].SetActive(true);

			shrinkObjects[i].transform.position = positions[i];
			shrinkObjects[i].transform.rotation = rotations[i];
			shrinkObjects[i].transform.localScale = scales[i];
			shrinkObjects[i].GetComponent<ReshrikingEntity>().SetMultipler(1);
		}
	}

	public void GenerateMap()
	{
		string holderName = "GeneratedMap";
		if(transform.FindChild(holderName))
		{
			DestroyImmediate(transform.FindChild(holderName).gameObject);
		}

		//transform.lossyScale = Vector3.one * tileScale;
		tileMap = new Transform[MapSizeX,MapSizeY];
		Transform mapHolder = new GameObject(holderName).transform;
		mapHolder.parent = this.transform;

		for (int x = 0;x  <MapSizeX ; x++)
		{
			for (int y = 0; y < MapSizeY; y++) {
				//Vector3 tilePosition = new Vector3(- MapSize.x/2 + tileScale/2 + x * tileScale, 0, - MapSize.y/2 + tileScale/2 + y * tileScale);
				Vector3 tilePosition = CoordToPosition(x, y);
				GameObject newTile = (GameObject)GameObject.Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90));
				newTile.transform.localScale = Vector3.one * tileScale;

				ReshrikingEntity re = newTile.GetComponent<ReshrikingEntity>() as ReshrikingEntity;

				if(re != null){
					re.SetInitialScale();
				}

				if(Random.Range(0,101) < tileTrapPercentage){
					Material mat = Resources.Load("Material/Web_Tile") as Material;
					newTile.GetComponent<Renderer>().material = mat;
					newTile.tag = "TileTrap";
				}

				newTile.transform.parent = mapHolder;
				tileMap[x,y] = newTile.transform;
			}
		}

	}

	private Vector3 CoordToPosition(int x, int y)
	{
		return new Vector3 (-MapSizeX / 2f + 0.5f + x, 0, -MapSizeY / 2f + 0.5f + y) * tileScale;
	}

	public bool GetTileFromPosition(Vector3 position, out Transform tileTransf)
	{
		int x = Mathf.RoundToInt(position.x / tileScale + (MapSizeX - 1) / 2f);
		int y = Mathf.RoundToInt(position.z / tileScale + (MapSizeY - 1) / 2f);

		if(x > tileMap.GetLength(0) - 1 || y > tileMap.GetLength(1) - 1)
		{
			tileTransf = null;
			return false;
		}
		else
		{
			x = Mathf.Clamp (x, 0, tileMap.GetLength (0) -1);
			y = Mathf.Clamp (y, 0, tileMap.GetLength (1) -1);
			tileTransf = tileMap [x, y];
			return true;
		}
	}

}
