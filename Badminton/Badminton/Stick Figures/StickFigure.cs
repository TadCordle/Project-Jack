using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

namespace Badminton.Stick_Figures
{
	class StickFigure
	{
		private World world;

		// Ragdoll
		protected Body torso, head, leftUpperArm, rightUpperArm, leftLowerArm, rightLowerArm, leftUpperLeg, rightUpperLeg, leftLowerLeg, rightLowerLeg;
		protected Body gyro;
		private AngleJoint neck, leftShoulder, rightShoulder, leftElbow, rightElbow, leftHip, rightHip, leftKnee, rightKnee;
		private RevoluteJoint r_neck, r_leftShoulder, r_rightShoulder, r_leftElbow, r_rightElbow, r_leftHip, r_rightHip, r_leftKnee, r_rightKnee;
		private AngleJoint upright;

		// Limb control
		public Dictionary<Body, float> health;
		private float maxImpulse;

		private List<ForceWave> punches;

		// Position properties
		/// <summary>
		/// The torso's position
		/// </summary>
		public Vector2 Position { get { return torso.Position; } }

		/// <summary>
		/// The left hand's position
		/// </summary>
		public Vector2 LeftHandPosition
		{
			get
			{
				if (health[leftLowerArm] > 0)
					return leftLowerArm.Position + new Vector2((float)Math.Sin(leftLowerArm.Rotation), -(float)Math.Cos(leftLowerArm.Rotation)) * 7.5f * MainGame.PIXEL_TO_METER;
				else
					return -Vector2.One;
			}
		}
		
		/// <summary>
		/// The right hand's position
		/// </summary>
		public Vector2 RightHandPosition
		{
			get
			{
				if (health[rightLowerArm] > 0)
					return rightLowerArm.Position + new Vector2((float)Math.Sin(rightLowerArm.Rotation), -(float)Math.Cos(rightLowerArm.Rotation)) * 7.5f * MainGame.PIXEL_TO_METER;
				else
					return -Vector2.One;
			}
		}
		
		// Action flags
		public bool Crouching { get; set; }
		private int walkStage;

		private float attackAngle;
		private bool punching;
		private bool punchArm; // true=left, false=right
		private int punchStage;

		// Other
		private Color color;
		private bool onGround;
		private int groundCheck;
		private Category collisionCat;
		
		#region Creation

		/// <summary>
		/// Stick figure constructor
		/// </summary>
		/// <param name="world">The world to place the physics objects in</param>
		/// <param name="position">The position to place the center of the stick figure's torso</param>
		/// <param name="collisionCat">The collision category of the figure. Different players will have different collision categories.</param>
		/// <param name="c">The color of the stick figure</param>
		public StickFigure(World world, Vector2 position, Category collisionCat, Color c)
		{
			this.world = world;
			maxImpulse = 0.2f;
			Crouching = false;
			health = new Dictionary<Body, float>();
			this.color = c;

			walkStage = 0;

			attackAngle = 0f;
			punching = false;
			punchArm = false;
			punchStage = 0;
			punches = new List<ForceWave>();

			groundCheck = 0;
			this.collisionCat = collisionCat;

			GenerateBody(world, position, collisionCat);
			ConnectBody(world);

			// TODO: Replace with ray tracing for more consistent ground detection
			onGround = false;
			leftLowerLeg.OnCollision += new OnCollisionEventHandler(OnGroundCollision);
			rightLowerLeg.OnCollision += new OnCollisionEventHandler(OnGroundCollision);
			leftLowerLeg.OnSeparation += new OnSeparationEventHandler(OnGroundSeparation);
			rightLowerLeg.OnSeparation += new OnSeparationEventHandler(OnGroundSeparation);

			head.OnCollision += new OnCollisionEventHandler(OtherCollisions);
			torso.OnCollision += new OnCollisionEventHandler(OtherCollisions);
			leftUpperArm.OnCollision += new OnCollisionEventHandler(OtherCollisions);
			leftLowerArm.OnCollision += new OnCollisionEventHandler(OtherCollisions);
			rightUpperArm.OnCollision += new OnCollisionEventHandler(OtherCollisions);
			rightLowerArm.OnCollision += new OnCollisionEventHandler(OtherCollisions);
			leftUpperLeg.OnCollision += new OnCollisionEventHandler(OtherCollisions);
			leftLowerLeg.OnCollision += new OnCollisionEventHandler(OtherCollisions);
			rightUpperLeg.OnCollision += new OnCollisionEventHandler(OtherCollisions);
			rightLowerLeg.OnCollision += new OnCollisionEventHandler(OtherCollisions);

			Stand();
		}

		/// <summary>
		/// Generates the stick figure's limbs, torso, and head
		/// </summary>
		/// <param name="world">The physics world to add the bodies to</param>
		/// <param name="position">The position to place the center of the torso</param>
		/// <param name="collisionCat">The collision category of the stick figure</param>
		protected void GenerateBody(World world, Vector2 position, Category collisionCat)
		{
			torso = BodyFactory.CreateCapsule(world, 40 * MainGame.PIXEL_TO_METER, 5 * MainGame.PIXEL_TO_METER, 10.0f);
			torso.Position = position;
			torso.BodyType = BodyType.Dynamic;
			torso.CollisionCategories = collisionCat;
			torso.CollidesWith = Category.All & ~collisionCat;
			gyro = BodyFactory.CreateBody(world, torso.Position);
			gyro.CollidesWith = Category.None;
			gyro.BodyType = BodyType.Dynamic;
			gyro.Mass = 0.00001f;
			gyro.FixedRotation = true;
			health.Add(torso, 1.0f);

			head = BodyFactory.CreateCircle(world, 12.5f * MainGame.PIXEL_TO_METER, 1.0f);
			head.Position = torso.Position - new Vector2(0, 29f) * MainGame.PIXEL_TO_METER;
			head.BodyType = BodyType.Dynamic;
			head.CollisionCategories = collisionCat;
			head.CollidesWith = Category.All & ~collisionCat;
			head.Restitution = 0.2f;
			health.Add(head, 1.0f);

			leftUpperArm = BodyFactory.CreateCapsule(world, 25 * MainGame.PIXEL_TO_METER, 5 * MainGame.PIXEL_TO_METER, 0.1f);
			leftUpperArm.Rotation = -MathHelper.PiOver2;
			leftUpperArm.Position = torso.Position + new Vector2(-7.5f, -15) * MainGame.PIXEL_TO_METER;
			leftUpperArm.BodyType = BodyType.Dynamic;
			leftUpperArm.CollisionCategories = collisionCat;
			leftUpperArm.CollidesWith = Category.Cat31; //Category.All & ~collisionCat;
			health.Add(leftUpperArm, 1.0f);

			rightUpperArm = BodyFactory.CreateCapsule(world, 25 * MainGame.PIXEL_TO_METER, 5 * MainGame.PIXEL_TO_METER, 0.1f);
			rightUpperArm.Rotation = MathHelper.PiOver2;
			rightUpperArm.Position = torso.Position + new Vector2(7.5f, -15) * MainGame.PIXEL_TO_METER;
			rightUpperArm.BodyType = BodyType.Dynamic;
			rightUpperArm.CollisionCategories = collisionCat;
			rightUpperArm.CollidesWith = Category.Cat31; //Category.All & ~collisionCat;
			health.Add(rightUpperArm, 1.0f);

			leftLowerArm = BodyFactory.CreateCapsule(world, 25 * MainGame.PIXEL_TO_METER, 5 * MainGame.PIXEL_TO_METER, 0.1f);
			leftLowerArm.Rotation = -MathHelper.PiOver2;
			leftLowerArm.Position = torso.Position + new Vector2(-22.5f, -15) * MainGame.PIXEL_TO_METER;
			leftLowerArm.BodyType = BodyType.Dynamic;
			leftLowerArm.CollisionCategories = collisionCat;
			leftLowerArm.CollidesWith = Category.Cat31; //Category.All & ~collisionCat;
			health.Add(leftLowerArm, 1.0f);

			rightLowerArm = BodyFactory.CreateCapsule(world, 25 * MainGame.PIXEL_TO_METER, 5 * MainGame.PIXEL_TO_METER, 0.1f);
			rightLowerArm.Rotation = MathHelper.PiOver2;
			rightLowerArm.Position = torso.Position + new Vector2(22.5f, -15) * MainGame.PIXEL_TO_METER;
			rightLowerArm.BodyType = BodyType.Dynamic;
			rightLowerArm.CollisionCategories = collisionCat;
			rightLowerArm.CollidesWith = Category.Cat31; //Category.All & ~collisionCat;
			health.Add(rightLowerArm, 1.0f);

			leftUpperLeg = BodyFactory.CreateCapsule(world, 25 * MainGame.PIXEL_TO_METER, 5 * MainGame.PIXEL_TO_METER, 5f);
			leftUpperLeg.Rotation = -3 * MathHelper.PiOver4;
			leftUpperLeg.Position = torso.Position + new Vector2(-25f / (float)Math.Sqrt(8) + 4, 10 + 25f / (float)Math.Sqrt(8)) * MainGame.PIXEL_TO_METER;
			leftUpperLeg.BodyType = BodyType.Dynamic;
			leftUpperLeg.CollisionCategories = collisionCat;
			leftUpperLeg.CollidesWith = Category.All & ~collisionCat;
			leftUpperLeg.Restitution = 0.15f;
			health.Add(leftUpperLeg, 1.0f);

			rightUpperLeg = BodyFactory.CreateCapsule(world, 25 * MainGame.PIXEL_TO_METER, 5 * MainGame.PIXEL_TO_METER, 5f);
			rightUpperLeg.Rotation = 3 * MathHelper.PiOver4;
			rightUpperLeg.Position = torso.Position + new Vector2(25f / (float)Math.Sqrt(8) - 4, 10 + 25f / (float)Math.Sqrt(8)) * MainGame.PIXEL_TO_METER;
			rightUpperLeg.BodyType = BodyType.Dynamic;
			rightUpperLeg.CollisionCategories = collisionCat;
			rightUpperLeg.CollidesWith = Category.All & ~collisionCat;
			rightUpperLeg.Restitution = 0.15f;
			health.Add(rightUpperLeg, 1.0f);

			leftLowerLeg = BodyFactory.CreateCapsule(world, 25 * MainGame.PIXEL_TO_METER, 5 * MainGame.PIXEL_TO_METER, 10.0f);
			leftLowerLeg.Position = torso.Position + new Vector2(-50f / (float)Math.Sqrt(8) + 6, 25 + 25f / (float)Math.Sqrt(8)) * MainGame.PIXEL_TO_METER;
			leftLowerLeg.BodyType = BodyType.Dynamic;
			leftLowerLeg.CollisionCategories = collisionCat;
			leftLowerLeg.CollidesWith = Category.All & ~collisionCat;
			leftLowerLeg.Restitution = 0.15f;
			leftLowerLeg.Friction = 3.0f;
			health.Add(leftLowerLeg, 1.0f);

			rightLowerLeg = BodyFactory.CreateCapsule(world, 25 * MainGame.PIXEL_TO_METER, 5 * MainGame.PIXEL_TO_METER, 10.0f);
			rightLowerLeg.Position = torso.Position + new Vector2(50f / (float)Math.Sqrt(8) - 6, 25 + 25f / (float)Math.Sqrt(8)) * MainGame.PIXEL_TO_METER;
			rightLowerLeg.BodyType = BodyType.Dynamic;
			rightLowerLeg.CollisionCategories = collisionCat;
			rightLowerLeg.CollidesWith = Category.All & ~collisionCat;
			rightLowerLeg.Restitution = 0.15f;
			rightLowerLeg.Friction = 3.0f;
			health.Add(rightLowerLeg, 1.0f);
		}

		/// <summary>
		/// Connects the figure's body parts
		/// </summary>
		/// <param name="world">The physics world to add the joints to</param>
		protected void ConnectBody(World world)
		{
			upright = JointFactory.CreateAngleJoint(world, torso, gyro);
			upright.MaxImpulse = maxImpulse * 0.2f;
			upright.TargetAngle = 0.0f;
			upright.CollideConnected = false;

			r_neck = JointFactory.CreateRevoluteJoint(world, head, torso, -Vector2.UnitY * 20 * MainGame.PIXEL_TO_METER);
			neck = JointFactory.CreateAngleJoint(world, head, torso);
			neck.CollideConnected = false;
			neck.MaxImpulse = maxImpulse;

			r_leftShoulder = JointFactory.CreateRevoluteJoint(world, leftUpperArm, torso, -Vector2.UnitY * 15 * MainGame.PIXEL_TO_METER);
			leftShoulder = JointFactory.CreateAngleJoint(world, leftUpperArm, torso);
			leftShoulder.CollideConnected = false;
			leftShoulder.MaxImpulse = maxImpulse;

			r_rightShoulder = JointFactory.CreateRevoluteJoint(world, rightUpperArm, torso, -Vector2.UnitY * 15 * MainGame.PIXEL_TO_METER);
			rightShoulder = JointFactory.CreateAngleJoint(world, rightUpperArm, torso);
			rightShoulder.CollideConnected = false;
			rightShoulder.MaxImpulse = maxImpulse;

			r_leftElbow = JointFactory.CreateRevoluteJoint(world, leftLowerArm, leftUpperArm, -Vector2.UnitY * 7.5f * MainGame.PIXEL_TO_METER);
			leftElbow = JointFactory.CreateAngleJoint(world, leftLowerArm, leftUpperArm);
			leftElbow.CollideConnected = false;
			leftElbow.MaxImpulse = maxImpulse;

			r_rightElbow = JointFactory.CreateRevoluteJoint(world, rightLowerArm, rightUpperArm, -Vector2.UnitY * 7.5f * MainGame.PIXEL_TO_METER);
			rightElbow = JointFactory.CreateAngleJoint(world, rightLowerArm, rightUpperArm);
			rightElbow.CollideConnected = false;
			rightElbow.MaxImpulse = maxImpulse;

			r_leftHip = JointFactory.CreateRevoluteJoint(world, leftUpperLeg, torso, Vector2.UnitY * 15 * MainGame.PIXEL_TO_METER);
			leftHip = JointFactory.CreateAngleJoint(world, leftUpperLeg, torso);
			leftHip.CollideConnected = false;
			leftHip.MaxImpulse = maxImpulse;

			r_rightHip = JointFactory.CreateRevoluteJoint(world, rightUpperLeg, torso, Vector2.UnitY * 15 * MainGame.PIXEL_TO_METER);
			rightHip = JointFactory.CreateAngleJoint(world, rightUpperLeg, torso);
			rightHip.CollideConnected = false;
			rightHip.MaxImpulse = maxImpulse;

			r_leftKnee = JointFactory.CreateRevoluteJoint(world, leftLowerLeg, leftUpperLeg, -Vector2.UnitY * 7.5f * MainGame.PIXEL_TO_METER);
			leftKnee = JointFactory.CreateAngleJoint(world, leftUpperLeg, leftLowerLeg);
			leftKnee.CollideConnected = false;
			leftKnee.MaxImpulse = maxImpulse;

			r_rightKnee = JointFactory.CreateRevoluteJoint(world, rightLowerLeg, rightUpperLeg, -Vector2.UnitY * 7.5f * MainGame.PIXEL_TO_METER);
			rightKnee = JointFactory.CreateAngleJoint(world, rightUpperLeg, rightLowerLeg);
			rightKnee.CollideConnected = false;
			rightKnee.MaxImpulse = maxImpulse;
		}

		#endregion

		#region Collision handlers

		private bool OnGroundCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			Vector2 normal = contact.Manifold.LocalNormal;
			if (normal.X == 0 || normal.Y / normal.X > 1)
				onGround = true;
			return contact.IsTouching();
		}
		private void OnGroundSeparation(Fixture fixtureA, Fixture fixtureB)
		{
			onGround = false;
		}

		private bool OtherCollisions(Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			if (fixtureB.Body.UserData is ForceWave)
			{
				ForceWave f = (ForceWave)fixtureB.Body.UserData;
				health[fixtureA.Body] -= 0.05f;
				fixtureB.Body.UserData = null;
				if (punches.Contains(f))
					punches.Remove(f);
			
				// TODO: Play punch sound
			}

			return true;
		}

		#endregion

		#region Actions

		/// <summary>
		/// Sends the stick figure to its default pose
		/// </summary>
		public void Stand()
		{
			Crouching = false;
			upright.TargetAngle = 0.0f;
			walkStage = 0;
			leftHip.TargetAngle = 3 * MathHelper.PiOver4;
			leftKnee.TargetAngle = -5 * MathHelper.PiOver4;
			rightHip.TargetAngle = -3 * MathHelper.PiOver4;
			rightKnee.TargetAngle = -3 * MathHelper.PiOver4;
			leftLowerLeg.Friction = 0f;
			rightLowerLeg.Friction = 0f;
			AngleJoint[] checkThese = new AngleJoint[] { leftHip, leftKnee, rightHip, rightKnee };
			if (JointsAreInPosition(checkThese))
			{
				leftLowerLeg.Friction = 1f;
				rightLowerLeg.Friction = 1f;
			}
		}

		/// <summary>
		/// Makes figure walk the the right (place in Update method)
		/// TODO: Add parameter to change walk speed
		/// </summary>
		public void WalkRight()
		{
			upright.TargetAngle = -0.1f;
			if (torso.LinearVelocity.X < (onGround ? 4 : 3) && !(Crouching && onGround))
				torso.ApplyForce(new Vector2(150, 0) * maxImpulse); // Change limb dependency
			AngleJoint[] checkThese = new AngleJoint[] { leftHip, rightHip };
			if (walkStage == 0)
			{
				leftHip.TargetAngle = (float)Math.PI - torso.Rotation;
				leftKnee.TargetAngle = -MathHelper.PiOver2 - torso.Rotation;
				rightHip.TargetAngle = -3 * MathHelper.PiOver4 - torso.Rotation;
				rightKnee.TargetAngle = -3 * MathHelper.PiOver4 - torso.Rotation;
				rightKnee.MaxImpulse = maxImpulse * 3 * health[rightLowerLeg] * health[rightUpperLeg];
				leftLowerLeg.Friction = 0.0f;
				rightLowerLeg.Friction = 1000f;
				if (JointsAreInPosition(checkThese))
					walkStage = 1;
			}
			else if (walkStage == 1)
			{
				leftHip.TargetAngle = 3 * MathHelper.PiOver2 - torso.Rotation;
				leftKnee.TargetAngle = -MathHelper.PiOver2 - torso.Rotation;
				rightHip.TargetAngle = -5 * MathHelper.PiOver4 - torso.Rotation;
				rightKnee.TargetAngle = -(float)Math.PI - torso.Rotation;
				rightKnee.MaxImpulse = maxImpulse * health[rightLowerLeg] * health[rightUpperLeg];
				if (JointsAreInPosition(checkThese))
					walkStage = 2;
			}
			else if (walkStage == 2)
			{
				leftHip.TargetAngle = 5 * MathHelper.PiOver4 - torso.Rotation;
				leftKnee.TargetAngle = -3 * MathHelper.PiOver4 - torso.Rotation;
				leftKnee.MaxImpulse = maxImpulse * 3 * health[leftLowerLeg] * health[leftUpperLeg];
				rightHip.TargetAngle = -(float)Math.PI - torso.Rotation;
				rightKnee.TargetAngle = -MathHelper.PiOver2 - torso.Rotation;
				rightLowerLeg.Friction = 0.0f;
				leftLowerLeg.Friction = 1000f;
				if (JointsAreInPosition(checkThese))
					walkStage = 3;
			}
			else if (walkStage == 3)
			{
				leftHip.TargetAngle = 3 * MathHelper.PiOver4 - torso.Rotation;
				leftKnee.TargetAngle = -(float)Math.PI - torso.Rotation;
				leftKnee.MaxImpulse = maxImpulse * health[leftLowerLeg] * health[leftUpperLeg];
				rightHip.TargetAngle = -MathHelper.PiOver2 - torso.Rotation;
				rightKnee.TargetAngle = -MathHelper.PiOver2 - torso.Rotation;
				if (JointsAreInPosition(checkThese))
					walkStage = 0;
			}
		}
	
		/// <summary>
		/// Makes figure walk to the left (place in Update method)
		/// TODO: Add parameter to change walk speed
		/// </summary>
		public void WalkLeft()
		{
			upright.TargetAngle = 0.1f;
			if (torso.LinearVelocity.X > (onGround ? -4 : -3))
				torso.ApplyForce(new Vector2(-150, 0) * maxImpulse); // Change limb dependency
			AngleJoint[] checkThese = new AngleJoint[] { leftHip, rightHip };
			if (walkStage == 0)
			{
				rightHip.TargetAngle = -(float)Math.PI - torso.Rotation;
				rightKnee.TargetAngle = -3 * MathHelper.PiOver2 - torso.Rotation;
				leftHip.TargetAngle = 3 * MathHelper.PiOver4 - torso.Rotation;
				leftKnee.TargetAngle = -5 * MathHelper.PiOver4 - torso.Rotation;
				leftKnee.MaxImpulse = maxImpulse * 3 * health[leftLowerLeg] * health[leftUpperLeg];
				leftLowerLeg.Friction = 1000.0f;
				rightLowerLeg.Friction = 0f;
				if (JointsAreInPosition(checkThese))
					walkStage = 1;
			}
			else if (walkStage == 1)
			{
				rightHip.TargetAngle = -3 * MathHelper.PiOver2 - torso.Rotation;
				rightKnee.TargetAngle = -3 * MathHelper.PiOver2 - torso.Rotation;
				leftHip.TargetAngle = 5 * MathHelper.PiOver4 - torso.Rotation;
				leftKnee.TargetAngle = -(float)Math.PI - torso.Rotation;
				leftKnee.MaxImpulse = maxImpulse * health[leftLowerLeg] * health[leftUpperLeg];
				if (JointsAreInPosition(checkThese))
					walkStage = 2;
			}
			else if (walkStage == 2)
			{
				rightHip.TargetAngle = -5 * MathHelper.PiOver4 - torso.Rotation;
				rightKnee.TargetAngle = -5 * MathHelper.PiOver4 - torso.Rotation;
				rightKnee.MaxImpulse = maxImpulse * 3 * health[rightLowerLeg] * health[rightUpperLeg];
				leftHip.TargetAngle = (float)Math.PI - torso.Rotation;
				leftKnee.TargetAngle = -3 * MathHelper.PiOver2 - torso.Rotation;
				leftLowerLeg.Friction = 0.0f;
				rightLowerLeg.Friction = 1000f;
				if (JointsAreInPosition(checkThese))
					walkStage = 3;
			}
			else if (walkStage == 3)
			{
				rightHip.TargetAngle = -3 * MathHelper.PiOver4 - torso.Rotation;
				rightKnee.TargetAngle = -(float)Math.PI - torso.Rotation;
				rightKnee.MaxImpulse = maxImpulse * health[rightLowerLeg] * health[rightUpperLeg];
				leftHip.TargetAngle = MathHelper.PiOver2 - torso.Rotation;
				leftKnee.TargetAngle = -3 * MathHelper.PiOver2 - torso.Rotation;
				if (JointsAreInPosition(checkThese))
					walkStage = 0;
			}
		}

		/// <summary>
		/// Makes the figure jump
		/// </summary>
		public void Jump()
		{
			upright.TargetAngle = 0.0f;
			leftHip.TargetAngle = MathHelper.Pi;
			leftKnee.TargetAngle = -MathHelper.Pi;
			rightHip.TargetAngle = -MathHelper.Pi;
			rightKnee.TargetAngle = -MathHelper.Pi;
			if (onGround)
			{
				leftLowerLeg.Friction = 100.0f;
				rightLowerLeg.Friction = 100.0f;
				torso.ApplyLinearImpulse(Vector2.UnitY * (Crouching ? -25 : -15)); // Change joint dependancy
			}
			Crouching = false;
		}

		/// <summary>
		/// Makes the figure crouch
		/// </summary>
		public void Crouch()
		{
			leftLowerLeg.Friction = 3f;
			rightLowerLeg.Friction = 3f;
			upright.TargetAngle = 0.0f;
			leftHip.TargetAngle = MathHelper.PiOver4;
			leftKnee.TargetAngle = -7 * MathHelper.PiOver4;
			rightHip.TargetAngle = -MathHelper.PiOver4;
			rightKnee.TargetAngle = -MathHelper.PiOver4;
		}

		/// <summary>
		/// Punches
		/// </summary>
		public void Punch(float angle)
		{
			punching = true;
			attackAngle = angle;
		}

		/// <summary>
		/// Checks if all the joints in a list are close to their target angle
		/// </summary>
		/// <param name="joints">The array of joints to check</param>
		/// <returns>True if the joints are at their target angles, false if not</returns>
		private bool JointsAreInPosition(AngleJoint[] joints)
		{
			foreach (AngleJoint j in joints)
			{
				if (Math.Abs(j.BodyB.Rotation - j.BodyA.Rotation - j.TargetAngle) > 0.20)
					return false;
			}
			return true;
		}

		#endregion

		#region Updating

		/// <summary>
		/// Updates some of the stick figures' key stances
		/// </summary>
		public virtual void Update()
		{
/*			List<Body> bodies = health.Keys.ToList<Body>();
			foreach (Body b in bodies)
			{
				health[b] -= 0.002f;
			}*/

			UpdateArms();

			List<ForceWave> toRemove = new List<ForceWave>();
			foreach (ForceWave f in punches)
			{
				if (f.body.UserData == null)
					toRemove.Add(f);
				f.Update();
			}

			foreach (ForceWave f in toRemove)
				punches.Remove(f);

			UpdateLimbStrength();
			
			if (Crouching)
				Crouch();

			if (Math.Abs(torso.LinearVelocity.Y) < 0.01f)
				groundCheck++;
			else
				groundCheck = 0;
			if (groundCheck >= 5)
				onGround = true;
		}

		/// <summary>
		/// Orients arms in necessary position
		/// </summary>
		private void UpdateArms()
		{
			if (!punching)
			{
				leftShoulder.TargetAngle = 3 * MathHelper.PiOver4; //FindClosestAngle(3 * MathHelper.PiOver4, torso.Rotation, leftShoulder.TargetAngle);
				rightShoulder.TargetAngle = -3 * MathHelper.PiOver4; //FindClosestAngle(-3 * MathHelper.PiOver4, torso.Rotation, rightShoulder.TargetAngle);
				leftElbow.TargetAngle = MathHelper.PiOver4;
				rightElbow.TargetAngle = -MathHelper.PiOver4;
			}
			else
			{
				AngleJoint[] checkThese = new AngleJoint[] { (punchArm ? leftShoulder : rightShoulder) };
				if (punchStage == -1)
				{
					punchStage = 0;
					float angle = attackAngle - MathHelper.PiOver2;
					if (punchArm)
					{
						leftShoulder.TargetAngle = GetArmTargetAngle(angle, true); // FindClosestAngle(angle, torso.Rotation, leftShoulder.TargetAngle);
						leftElbow.TargetAngle = 0f;
						leftShoulder.MaxImpulse = 1000f;
						leftElbow.MaxImpulse = 1000f;
					}
					else
					{
						rightShoulder.TargetAngle = GetArmTargetAngle(angle, false); // FindClosestAngle(angle, torso.Rotation, rightShoulder.TargetAngle);
						rightElbow.TargetAngle = 0f;
						rightShoulder.MaxImpulse = 1000f;
						rightElbow.MaxImpulse = 1000f;
					}
				}
				else if (punchStage == 0)
				{
					if (JointsAreInPosition(checkThese))
					{
						float angle = attackAngle - MathHelper.PiOver2;
						if (punchArm)
						{
							leftShoulder.TargetAngle = 3 * MathHelper.PiOver4; // FindClosestAngle(3 * MathHelper.PiOver4, torso.Rotation, leftShoulder.TargetAngle);
							leftElbow.TargetAngle = MathHelper.PiOver4;
							leftShoulder.MaxImpulse = maxImpulse;
							leftElbow.MaxImpulse = maxImpulse;
							punches.Add(new ForceWave(world, LeftHandPosition, new Vector2(-(float)Math.Sin(angle), -(float)Math.Cos(angle)) * 10, this.collisionCat));
						}
						else
						{
							rightShoulder.TargetAngle = -3 * MathHelper.PiOver4; // FindClosestAngle(-3 * MathHelper.PiOver4, torso.Rotation, rightShoulder.TargetAngle);
							rightElbow.TargetAngle = -MathHelper.PiOver4;
							rightShoulder.MaxImpulse = maxImpulse;
							rightElbow.MaxImpulse = maxImpulse;
							punches.Add(new ForceWave(world, RightHandPosition, new Vector2(-(float)Math.Sin(angle), -(float)Math.Cos(angle)) * 10, this.collisionCat));
						}
						punchStage = 1;
					}
				}
				else if (punchStage == 1)
				{
					if (JointsAreInPosition(checkThese))
					{
						punchArm = !punchArm;
						punching = false;
						punchStage = -1;
					}
				}
			}
		}

		/// <summary>
		/// Changes the MaxImpulse of each joint based on the health of its limbs
		/// TODO: More balancing
		/// </summary>
		private void UpdateLimbStrength()
		{
			List<Body> bodies = health.Keys.ToList();
			foreach (Body b in bodies)
				health[b] = Math.Max(health[b], 0f);
			upright.MaxImpulse = maxImpulse * health[torso] * health[head];
			neck.MaxImpulse = maxImpulse * health[head] * health[torso];
			leftShoulder.MaxImpulse = maxImpulse * health[torso] * health[leftUpperArm] * health[head];
			leftElbow.MaxImpulse = maxImpulse * health[leftUpperArm] * health[leftLowerArm] * health[torso] * health[head];
			rightShoulder.MaxImpulse = maxImpulse * health[torso] * health[rightUpperArm] * health[head];
			rightElbow.MaxImpulse = maxImpulse * health[rightUpperArm] * health[rightLowerArm] * health[torso] * health[head];
			leftHip.MaxImpulse = maxImpulse * health[torso] * health[leftUpperLeg] * health[head];
			leftKnee.MaxImpulse = maxImpulse * health[leftUpperLeg] * health[leftLowerLeg] * health[torso] * health[head];
			rightHip.MaxImpulse = maxImpulse * health[torso] * health[rightUpperLeg] * health[head];
			rightKnee.MaxImpulse = maxImpulse * health[rightUpperLeg] * health[rightLowerLeg] * health[torso] * health[head];
		}

		#endregion

		#region Helpers/debug

		public void ApplyForce(Vector2 v)
		{
			torso.ApplyForce(v * 10);
		}

		/// <summary>
		/// Finds the closest physical angle to a pair of numerical angles which may vary by more than 2pi
		/// </summary>
		/// <param name="physAngle">The physical angle you wish to achieve</param>
		/// <param name="numAngle1">The first numerical angle</param>
		/// <param name="numAngle2">The second numerical angle</param>
		/// <returns>A numerical angle which is physically the same as physAngle</returns>
		private float FindClosestAngle(float physAngle, float numAngle1, float numAngle2)
		{
			physAngle += numAngle1;
			while (physAngle - numAngle2 > Math.PI)
				physAngle -= MathHelper.TwoPi;
			while (physAngle - numAngle2 < -Math.PI)
				physAngle += MathHelper.TwoPi;
			return physAngle;
		}

		private float GetArmTargetAngle(float physAngle, bool leftArm)
		{
			if (leftArm)
			{
				if (physAngle > 0f)
					return physAngle;
				else
					return physAngle + MathHelper.TwoPi;
			}
			else
			{
				if (physAngle > 0f)
					return physAngle - MathHelper.TwoPi;
				else
					return physAngle;
			}
		}

		#endregion

		#region Drawing

		/// <summary>
		/// Draws the stick figure
		/// </summary>
		/// <param name="sb">The SpriteBatch used to draw the stick figure</param>
		public virtual void Draw(SpriteBatch sb)
		{
			Color deathColor = Color.Black;
			Color c = Blend(color, deathColor, health[torso]);
			sb.Draw(MainGame.tex_torso, torso.Position * MainGame.METER_TO_PIXEL * MainGame.RESOLUTION_SCALE, null, c, torso.Rotation, new Vector2(5f, 20f), MainGame.RESOLUTION_SCALE, SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[leftUpperArm]);
			sb.Draw(MainGame.tex_limb, leftUpperArm.Position * MainGame.METER_TO_PIXEL * MainGame.RESOLUTION_SCALE, null, c, leftUpperArm.Rotation, new Vector2(5f, 12.5f), MainGame.RESOLUTION_SCALE, SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[rightUpperArm]);
			sb.Draw(MainGame.tex_limb, rightUpperArm.Position * MainGame.METER_TO_PIXEL * MainGame.RESOLUTION_SCALE, null, c, rightUpperArm.Rotation, new Vector2(5f, 12.5f), MainGame.RESOLUTION_SCALE, SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[leftLowerArm]);
			sb.Draw(MainGame.tex_limb, leftLowerArm.Position * MainGame.METER_TO_PIXEL * MainGame.RESOLUTION_SCALE, null, c, leftLowerArm.Rotation, new Vector2(5f, 12.5f), MainGame.RESOLUTION_SCALE, SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[rightLowerArm]);
			sb.Draw(MainGame.tex_limb, rightLowerArm.Position * MainGame.METER_TO_PIXEL * MainGame.RESOLUTION_SCALE, null, c, rightLowerArm.Rotation, new Vector2(5f, 12.5f), MainGame.RESOLUTION_SCALE, SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[leftUpperLeg]);
			sb.Draw(MainGame.tex_limb, leftUpperLeg.Position * MainGame.METER_TO_PIXEL * MainGame.RESOLUTION_SCALE, null, c, leftUpperLeg.Rotation, new Vector2(5f, 12.5f), MainGame.RESOLUTION_SCALE, SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[rightUpperLeg]);
			sb.Draw(MainGame.tex_limb, rightUpperLeg.Position * MainGame.METER_TO_PIXEL * MainGame.RESOLUTION_SCALE, null, c, rightUpperLeg.Rotation, new Vector2(5f, 12.5f), MainGame.RESOLUTION_SCALE, SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[leftLowerLeg]);
			sb.Draw(MainGame.tex_limb, leftLowerLeg.Position * MainGame.METER_TO_PIXEL * MainGame.RESOLUTION_SCALE, null, c, leftLowerLeg.Rotation, new Vector2(5f, 12.5f), MainGame.RESOLUTION_SCALE, SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[rightLowerLeg]);
			sb.Draw(MainGame.tex_limb, rightLowerLeg.Position * MainGame.METER_TO_PIXEL * MainGame.RESOLUTION_SCALE, null, c, rightLowerLeg.Rotation, new Vector2(5f, 12.5f), MainGame.RESOLUTION_SCALE, SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[head]);
			sb.Draw(MainGame.tex_head, head.Position * MainGame.METER_TO_PIXEL * MainGame.RESOLUTION_SCALE, null, c, head.Rotation, new Vector2(12.5f, 12.5f), MainGame.RESOLUTION_SCALE, SpriteEffects.None, 0.0f);

			// Debug
//			sb.DrawString(MainGame.fnt_basicFont, "L", LeftHandPosition * MainGame.METER_TO_PIXEL, Color.Blue);
//			sb.DrawString(MainGame.fnt_basicFont, "R", RightHandPosition * MainGame.METER_TO_PIXEL, Color.Lime);
//			sb.DrawString(MainGame.fnt_basicFont, torso.Position.ToString(), Vector2.UnitY * 64, Color.White);
			foreach (ForceWave f in punches)
				f.Draw(sb);
		}

		/// <summary>Blends the specified colors together.</summary>
		/// <param name="color">Color to blend onto the background color.</param>
		/// <param name="backColor">Color to blend the other color onto.</param>
		/// <param name="amount">How much of <paramref name="color"/> to keep,
		/// “on top of” <paramref name="backColor"/>.</param>
		/// <returns>The blended colors.</returns>
		private Color Blend(Color c1, Color c2, float amount)
		{
			byte r = (byte)((c1.R * amount) + c2.R * (1 - amount));
			byte g = (byte)((c1.G * amount) + c2.G * (1 - amount));
			byte b = (byte)((c1.B * amount) + c2.B * (1 - amount));
			return Color.FromNonPremultiplied(r, g, b, c1.A);
		}

		#endregion
	}
}
