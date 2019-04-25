using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetMove : MonoBehaviour
{
        public Transform origin;    
	public float gspeed;        
	public float zspeed;        
	public float ry, rz;        

	void Start() {
	}

	void FixedUpdate () {

		Vector3 axis = new Vector3 (0, ry, rz);     
		this.transform.RotateAround (origin.position, axis, gspeed * Time.deltaTime);   
		this.transform.Rotate (Vector3.up * zspeed * Time.deltaTime);       
	} 
}
