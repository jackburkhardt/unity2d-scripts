using System;
using System.Collections.Generic;
using UnityEngine;
public class PhysicsObject : MonoBehaviour
    {
        public float minGroundNormalY = .65f;
        public float gravityModifier = 1f;

        // Is the player standing on the ground?
        protected bool grounded;
        protected Vector2 velocity;
        protected Rigidbody2D rb2d;
        // Collision filtering mask to make sure we measure what we want
        protected ContactFilter2D contactFilter;
        protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
        // List of object which will trigger our collider
        protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);
        protected Vector2 groundNormal;
        protected Vector2 targetVelocity;
        
        // Min distance to check for collision (don't want to check when no movement)
        protected const float minMoveDist = 0.001f;
        // Padding to make sure we do not get stuck in another collider
        protected const float shellRadius = 0.01f;

        
        private void OnEnable()
        {
            // Init assignment
            rb2d = GetComponent<Rigidbody2D>();
            // Assumes we start on ground, allows for some fixes of edge cases
            groundNormal = new Vector2(0f, 1f);
        }

        void Start()
        {
            // Filter config
            contactFilter.useTriggers = false;
            contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            contactFilter.useLayerMask = true;
        }

        private void Update()
        {
            // We re-calculate this each frame
            targetVelocity = Vector2.zero;
            ComputeVelocity();
        }

        protected virtual void ComputeVelocity()
        {
            // This is overriden in the player controller (since this is being used for player)
            // Once I need object physics I'll flesh this out
        }

        private void FixedUpdate()
        {
            velocity += Physics2D.gravity * (gravityModifier * Time.deltaTime);
            velocity.x = targetVelocity.x;
            
            grounded = false;
            
            Vector2 deltaPosition = velocity * Time.deltaTime; 
            Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);
            
            // We run the horizontal axis first for better behavior with slopes
            Vector2 move = moveAlongGround * deltaPosition.x;
            Movement(move, false);
            
            move = Vector2.up * deltaPosition.y;
            Movement(move,true);
        }

        void Movement(Vector2 move, bool yMovement)
        {
            float distance = move.magnitude;
            
            if (distance > minMoveDist)
            {
               int count = rb2d.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
               hitBufferList.Clear();
               for (int i = 0; i < count; i++)
               {
                   hitBufferList.Add(hitBuffer[i]);
               }

               for (int i = 0; i < hitBufferList.Count; i++)
               {
                   Vector2 currentNormal = hitBufferList[i].normal;
                   if (currentNormal.y > minGroundNormalY)
                   {
                       grounded = true;
                       if (yMovement)
                       {
                           groundNormal = currentNormal;
                           currentNormal.x = 0;
                       }
                   }

                   float projection = Vector2.Dot(velocity, currentNormal);
                   if (projection < 0)
                   {
                       velocity -= projection * currentNormal;
                   }

                   float modifiedDistance = hitBufferList[i].distance - shellRadius;
                   distance = modifiedDistance < distance ? modifiedDistance : distance;
               }
            }
            
            rb2d.position += move.normalized * distance;
        }
    }