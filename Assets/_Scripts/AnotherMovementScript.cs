using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnotherMovementScript : MonoBehaviour {

 public float speed = 6.0F;
 public float jumpSpeed = 20.0F;
 public float gravity = 20.0F;
 public float gravityForce = 3.0f; 
 public float airTime = 2f;
 private Vector3 moveDirection = Vector3.zero;
 private CharacterController controller;
 private float forceY = 0;
 private float invertGrav;
 
 void Start(){
     // invertGrav is set greater than gravity so that our guy jumps
     invertGrav = gravity * airTime;
     controller = GetComponent<CharacterController>();
 }
 void Update() {   
     moveDirection = new Vector3(Input.GetAxis("Horizontal"),0,0);
     moveDirection = transform.TransformDirection(moveDirection);
     moveDirection *= speed;    
     if (controller.isGrounded) {
         // we are grounded so forceY is 0
         forceY = 0;
         // invertGrav is also reset based on the gravity
         invertGrav = gravity * airTime;
         if (Input.GetKeyDown(KeyCode.Space)){
             // we jump 
             forceY = jumpSpeed;
         }
     }
     // We are now jumping since forceY is not 0
     // we add invertGrav to our jumpForce and invertGrav is also
     // decreased so that we get a curvy jump
     if(Input.GetKey(KeyCode.Space) && forceY != 0 ){
         invertGrav -= Time.deltaTime;
         forceY += invertGrav*Time.deltaTime;
     } 
     // Here we apply the gravity
     forceY -= gravity*Time.deltaTime* gravityForce;
     moveDirection.y = forceY;
     controller.Move(moveDirection * Time.deltaTime);
 }
}
