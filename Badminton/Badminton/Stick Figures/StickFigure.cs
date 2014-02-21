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

using Badminton.Attacks;

namespace Badminton.Stick_Figures
{
	class StickFigure
	{
		protected World world;

		// Ragdoll
		protected Body torso, head, leftUpperArm, rightUpperArm, leftLowerArm, rightLowerArm, leftUpperLeg, rightUpperLeg, leftLowerLeg, rightLowerLeg;
		protected Body gyro;
		private AngleJoint neck, leftShoulder, rightShoulder, leftElbow, rightElbow, leftHip, rightHip, leftKnee, rightKnee;
		private RevoluteJoint r_neck, r_leftShoulder, r_rightShoulder, r_leftElbow, r_rightElbow, r_leftHip, r_rightHip, r_leftKnee, r_rightKnee;
		private AngleJoint upright;

		// Limb control
		public Dictionary<Body, float> health;
		private Dictionary<Body, int> forceNextPos;
		private float maxImpulse;
		protected float limbStrength;
		protected float limbDefense;
		private float friction;

		private List<Attack> attacks;

		// Keep track of animation states
		private int walkStage;
		private int punchStage, kickStage;
		private const int MIN_POSE_TIME = 30;

		// Attacking
		public static bool AllowTraps = true;
		public static bool AllowLongRange = true;
		private float attackAngle;
		private bool punching, kicking, aiming, throwing;
		private bool punchArm, kickLeg, throwArm; // true=left, false=right
		private int chargeUp, coolDown, trapAmmo, trapThrowTime;
		private const int MAX_CHARGE = 100, COOL_PERIOD = 30;
		private const int MAX_AMMO = 3, THROW_TIME = 40;

		// Other
		protected Vector2 startPosition;
		protected float scale;
		protected Color color;
		protected Category collisionCat;
		protected bool evilSkin;
		private Vector2 groundSensorStart, groundSensorEnd;
		private bool increaseFall;
		private bool leftLegLeft, rightLegLeft;

		#region Properties

		#region Position properties

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
					return leftLowerArm.Position + new Vector2((float)Math.Sin(leftLowerArm.Rotation), -(float)Math.Cos(leftLowerArm.Rotation)) * 7.5f * scale * MainGame.PIXEL_TO_METER;
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
					return rightLowerArm.Position + new Vector2((float)Math.Sin(rightLowerArm.Rotation), -(float)Math.Cos(rightLowerArm.Rotation)) * 7.5f * scale * MainGame.PIXEL_TO_METER;
				else
					return -Vector2.One;
			}
		}

		/// <summary>
		/// The left foot's position
		/// </summary>
		public Vector2 LeftFootPosition
		{
			get
			{
				if (health[leftLowerLeg] > 0)
					return leftLowerLeg.Position - new Vector2((float)Math.Sin(leftLowerLeg.Rotation), -(float)Math.Cos(leftLowerLeg.Rotation)) * 7.5f * scale * MainGame.PIXEL_TO_METER;
				else
					return -Vector2.One;
			}
		}

		/// <summary>
		/// The left knee's position
		/// </summary>
		public Vector2 LeftKneePosition
		{
			get
			{
				if (health[leftUpperLeg] > 0)
					return leftUpperLeg.Position + new Vector2((float)Math.Sin(leftUpperLeg.Rotation), -(float)Math.Cos(leftUpperLeg.Rotation)) * 7.5f * scale * MainGame.PIXEL_TO_METER;
				else
					return -Vector2.One;
			}
		}

		/// <summary>
		/// The right foot's position
		/// </summary>
		public Vector2 RightFootPosition
		{
			get
			{
				if (health[rightLowerLeg] > 0)
					return rightLowerLeg.Position - new Vector2((float)Math.Sin(rightLowerLeg.Rotation), -(float)Math.Cos(rightLowerLeg.Rotation)) * 7.5f * scale * MainGame.PIXEL_TO_METER;
				else
					return -Vector2.One;
			}
		}

		/// <summary>
		/// The left knee's position
		/// </summary>
		public Vector2 RightKneePosition
		{
			get
			{
				if (health[rightUpperLeg] > 0)
					return rightUpperLeg.Position + new Vector2((float)Math.Sin(rightUpperLeg.Rotation), -(float)Math.Cos(rightUpperLeg.Rotation)) * 7.5f * scale * MainGame.PIXEL_TO_METER;
				else
					return -Vector2.One;
			}
		}

		#endregion

		/// <summary>
		/// Whether or not the figure is dead
		/// </summary>
		public bool IsDead
		{
			get { return health[head] <= 0 || health[torso] <= 0; }
		}

		/// <summary>
		/// Whether or not the stick figure is crouching
		/// </summary>
		public bool Crouching { get; set; }

		/// <summary>
		/// Whether or not the stick figure in the jump stage
		/// </summary>
		public bool Jumping
		{
			get { return _jumping; }
			set
			{
				if (value && OnGround)
					increaseFall = true;
				_jumping = value;
			}
		}
		private bool _jumping;

		/// <summary>
		/// Returns whether or not the stick figure is standing on solid ground
		/// </summary>
		public bool OnGround
		{
			get
			{
				bool onGround = false;

				// Find limbs that 
				List<Vector2> checkThese = new List<Vector2>();
				if (health[leftLowerLeg] > 0)
					checkThese.Add(LeftFootPosition);
				else if (health[leftUpperLeg] > 0)
					checkThese.Add(LeftKneePosition);

				if (health[rightLowerLeg] > 0)
					checkThese.Add(RightFootPosition);
				else if (health[leftUpperLeg] > 0)
					checkThese.Add(RightKneePosition);

				if (checkThese.Count == 0)
					checkThese.Add(torso.Position + Vector2.UnitY * 17.5f * scale * MainGame.PIXEL_TO_METER);

				foreach (Vector2 v in checkThese)
				{
					groundSensorStart = v;
					groundSensorEnd = groundSensorStart + new Vector2(0, 20 * MainGame.PIXEL_TO_METER * scale);
					world.RayCast((f, p, n, fr) =>
					{
						if (f != null && f.Body.UserData is Wall)
						{
							onGround = true;
							return 0;
						}
						else
						{
							onGround = false;
							return -1;
						}
					}, groundSensorStart, groundSensorEnd);

					if (onGround)
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Gets/sets which direction the stick figure was last facing
		/// </summary>
		protected bool LastFacedLeft { get; set; }

		/// <summary>
		/// Gets/sets whether or not the stick figure can perform any actions
		/// </summary>
		public bool LockControl { get; set; }

		/// <summary>
		/// Returns the collision category (or team) of the stick figure
		/// </summary>
		public Category CollisionCategory { get { return collisionCat; } }

		/// <summary>
		/// Sets the color of the stick figure
		/// </summary>
		public Color Color { set { this.color = value; } get { return this.color; } }

		/// <summary>
		/// Gets the overall health of the stick figure (0=0%, 1=100%)
		/// </summary>
		public float ScalarHealth
		{
			get
			{
				return (0.1f + (health[leftLowerArm] + health[leftUpperArm] + health[rightLowerArm] + health[rightUpperArm] + health[leftLowerLeg] + health[leftUpperLeg] + health[rightLowerLeg] + health[rightUpperLeg]) * 9 / 80f) *
						health[head] * health[torso];
			}
		}

		/// <summary>
		/// Returns the amount of ammo the stick figure has
		/// </summary>
		public int TrapAmmo { get { return trapAmmo; } }

		#endregion

		#region Creation/Destruction

		/// <summary>
		/// Stick figure constructor
		/// </summary>
		/// <param name="world">The world to place the physics objects in</param>
		/// <param name="position">The position to place the center of the stick figure's torso</param>
		/// <param name="collisionCat">The collision category of the figure. Different players will have different collision categories.</param>
		/// <param name="scale">Scales the size of the stick figure</param>
		/// <param name="limbStrength">Changes how well the stick figure performs its actions</param>
		/// <param name="evilSkin">Whether or not to draw with the darkers skin</param>
		/// <param name="health">Scales how much damage the stick figure takes from attacks</param>
		/// <param name="c">The color of the stick figure</param>
		public StickFigure(World world, Vector2 position, Category collisionCat, float scale, float limbStrength, float limbDefense, bool evilSkin, Color c)
		{
			this.world = world;
			this.startPosition = position;
			this.limbStrength = limbStrength;
			this.maxImpulse = 0.2f * scale * scale * limbStrength;
			this.limbDefense = limbDefense;
			this.evilSkin = evilSkin;
			friction = 5f * scale;
			Crouching = false;
			this.health = new Dictionary<Body, float>();
			this.forceNextPos = new Dictionary<Body, int>();
			this.color = c;
			this.scale = scale;
			this.increaseFall = false;
			_jumping = false;

			walkStage = 0;

			attackAngle = 0f;
			punching = kicking = false;
			punchArm = kickLeg = false;
			punchStage = kickStage = -1;
			chargeUp = 0;
			trapAmmo = MAX_AMMO;
			trapThrowTime = THROW_TIME;
			attacks = new List<Attack>();

			this.collisionCat = collisionCat;
			LastFacedLeft = true;
			LockControl = false;

			GenerateBody(world, position, collisionCat);
			ConnectBody(world);

			head.OnCollision += new OnCollisionEventHandler(SpecialCollisions);
			leftUpperArm.OnCollision += new OnCollisionEventHandler(SpecialCollisions);
			leftLowerArm.OnCollision += new OnCollisionEventHandler(SpecialCollisions);
			rightUpperArm.OnCollision += new OnCollisionEventHandler(SpecialCollisions);
			rightLowerArm.OnCollision += new OnCollisionEventHandler(SpecialCollisions);
			leftUpperLeg.OnCollision += new OnCollisionEventHandler(SpecialCollisions);
			leftLowerLeg.OnCollision += new OnCollisionEventHandler(SpecialCollisions);
			rightUpperLeg.OnCollision += new OnCollisionEventHandler(SpecialCollisions);
			rightLowerLeg.OnCollision += new OnCollisionEventHandler(SpecialCollisions);

			Stand();
		}

		/// <summary>
		/// Returns a new stick figure at its original spawn point
		/// </summary>
		/// <returns> a new stick figure at its original spawn point</returns>
		public virtual StickFigure Respawn()
		{
			return new StickFigure(world, startPosition, collisionCat, scale, limbStrength, limbDefense, evilSkin, color);
		}
	
		/// <summary>
		/// Generates the stick figure's limbs, torso, and head
		/// </summary>
		/// <param name="world">The physics world to add the bodies to</param>
		/// <param name="position">The position to place the center of the torso</param>
		/// <param name="collisionCat">The collision category of the stick figure</param>
		protected void GenerateBody(World world, Vector2 position, Category collisionCat)
		{
			torso = BodyFactory.CreateCapsule(world, 40 * scale * MainGame.PIXEL_TO_METER, 5 * scale * MainGame.PIXEL_TO_METER, 10.0f);
			torso.Position = position;
			torso.BodyType = BodyType.Dynamic;
			torso.CollisionCategories = collisionCat;
			torso.CollidesWith = Category.All & ~collisionCat;
			torso.Friction = friction;
			gyro = BodyFactory.CreateBody(world, torso.Position);
			gyro.CollidesWith = Category.None;
			gyro.BodyType = BodyType.Dynamic;
			gyro.Mass = 0.00001f;
			gyro.FixedRotation = true;
			health.Add(torso, 1.0f);
			forceNextPos.Add(torso, MIN_POSE_TIME);

			head = BodyFactory.CreateCircle(world, 12.5f * scale * MainGame.PIXEL_TO_METER, 1.0f);
			head.Position = torso.Position - new Vector2(0, 29f) * scale * MainGame.PIXEL_TO_METER;
			head.BodyType = BodyType.Dynamic;
			head.CollisionCategories = collisionCat;
			head.CollidesWith = Category.All & ~collisionCat;
			head.Restitution = 0.2f;
			head.Friction = friction;
			health.Add(head, 1.0f);
			forceNextPos.Add(head, MIN_POSE_TIME);

			leftUpperArm = BodyFactory.CreateCapsule(world, 25 * scale * MainGame.PIXEL_TO_METER, 5 * scale * MainGame.PIXEL_TO_METER, 0.1f);
			leftUpperArm.Rotation = -MathHelper.PiOver2;
			leftUpperArm.Position = torso.Position + new Vector2(-7.5f, -15) * scale * MainGame.PIXEL_TO_METER;
			leftUpperArm.BodyType = BodyType.Dynamic;
			leftUpperArm.CollisionCategories = collisionCat;
			leftUpperArm.CollidesWith = Category.All & ~collisionCat;
			leftUpperArm.Friction = friction;
			health.Add(leftUpperArm, 1.0f);
			forceNextPos.Add(leftUpperArm, MIN_POSE_TIME);

			rightUpperArm = BodyFactory.CreateCapsule(world, 25 * scale * MainGame.PIXEL_TO_METER, 5 * scale * MainGame.PIXEL_TO_METER, 0.1f);
			rightUpperArm.Rotation = MathHelper.PiOver2;
			rightUpperArm.Position = torso.Position + new Vector2(7.5f, -15) * scale * MainGame.PIXEL_TO_METER;
			rightUpperArm.BodyType = BodyType.Dynamic;
			rightUpperArm.CollisionCategories = collisionCat;
			rightUpperArm.CollidesWith = Category.All & ~collisionCat;
			rightUpperArm.Friction = friction;
			health.Add(rightUpperArm, 1.0f);
			forceNextPos.Add(rightUpperArm, MIN_POSE_TIME);

			leftLowerArm = BodyFactory.CreateCapsule(world, 25 * scale * MainGame.PIXEL_TO_METER, 5 * scale * MainGame.PIXEL_TO_METER, 0.1f);
			leftLowerArm.Rotation = -MathHelper.PiOver2;
			leftLowerArm.Position = torso.Position + new Vector2(-22.5f, -15) * scale * MainGame.PIXEL_TO_METER;
			leftLowerArm.BodyType = BodyType.Dynamic;
			leftLowerArm.CollisionCategories = collisionCat;
			leftLowerArm.CollidesWith = Category.All & ~collisionCat;
			leftLowerArm.Friction = friction;
			health.Add(leftLowerArm, 1.0f);
			forceNextPos.Add(leftLowerArm, MIN_POSE_TIME);

			rightLowerArm = BodyFactory.CreateCapsule(world, 25 * scale * MainGame.PIXEL_TO_METER, 5 * scale * MainGame.PIXEL_TO_METER, 0.1f);
			rightLowerArm.Rotation = MathHelper.PiOver2;
			rightLowerArm.Position = torso.Position + new Vector2(22.5f, -15) * scale * MainGame.PIXEL_TO_METER;
			rightLowerArm.BodyType = BodyType.Dynamic;
			rightLowerArm.CollisionCategories = collisionCat;
			rightLowerArm.CollidesWith = Category.All & ~collisionCat;
			rightLowerArm.Friction = friction;
			health.Add(rightLowerArm, 1.0f);
			forceNextPos.Add(rightLowerArm, MIN_POSE_TIME);

			leftUpperLeg = BodyFactory.CreateCapsule(world, 25 * scale * MainGame.PIXEL_TO_METER, 5 * scale * MainGame.PIXEL_TO_METER, 5f);
			leftUpperLeg.Rotation = -3 * MathHelper.PiOver4;
			leftUpperLeg.Position = torso.Position + new Vector2(-25f / (float)Math.Sqrt(8) + 4, 10 + 25f / (float)Math.Sqrt(8)) * scale * MainGame.PIXEL_TO_METER;
			leftUpperLeg.BodyType = BodyType.Dynamic;
			leftUpperLeg.CollisionCategories = collisionCat;
			leftUpperLeg.CollidesWith = Category.All & ~collisionCat;
			leftUpperLeg.Restitution = 0.15f;
			leftUpperLeg.Friction = friction;
			health.Add(leftUpperLeg, 1.0f);
			forceNextPos.Add(leftUpperLeg, MIN_POSE_TIME);

			rightUpperLeg = BodyFactory.CreateCapsule(world, 25 * scale * MainGame.PIXEL_TO_METER, 5 * scale * MainGame.PIXEL_TO_METER, 5f);
			rightUpperLeg.Rotation = 3 * MathHelper.PiOver4;
			rightUpperLeg.Position = torso.Position + new Vector2(25f / (float)Math.Sqrt(8) - 4, 10 + 25f / (float)Math.Sqrt(8)) * scale * MainGame.PIXEL_TO_METER;
			rightUpperLeg.BodyType = BodyType.Dynamic;
			rightUpperLeg.CollisionCategories = collisionCat;
			rightUpperLeg.CollidesWith = Category.All & ~collisionCat;
			rightUpperLeg.Restitution = 0.15f;
			rightUpperLeg.Friction = friction;
			health.Add(rightUpperLeg, 1.0f);
			forceNextPos.Add(rightUpperLeg, MIN_POSE_TIME);

			leftLowerLeg = BodyFactory.CreateCapsule(world, 25 * scale * MainGame.PIXEL_TO_METER, 5 * scale * MainGame.PIXEL_TO_METER, 10.0f);
			leftLowerLeg.Position = torso.Position + new Vector2(-50f / (float)Math.Sqrt(8) + 6, 25 + 25f / (float)Math.Sqrt(8)) * scale * MainGame.PIXEL_TO_METER;
			leftLowerLeg.BodyType = BodyType.Dynamic;
			leftLowerLeg.CollisionCategories = collisionCat;
			leftLowerLeg.CollidesWith = Category.All & ~collisionCat;
			leftLowerLeg.Restitution = 0.15f;
			leftLowerLeg.Friction = friction;
			health.Add(leftLowerLeg, 1.0f);
			forceNextPos.Add(leftLowerLeg, MIN_POSE_TIME);

			rightLowerLeg = BodyFactory.CreateCapsule(world, 25 * scale * MainGame.PIXEL_TO_METER, 5 * scale * MainGame.PIXEL_TO_METER, 10.0f);
			rightLowerLeg.Position = torso.Position + new Vector2(50f / (float)Math.Sqrt(8) - 6, 25 + 25f / (float)Math.Sqrt(8)) * scale * MainGame.PIXEL_TO_METER;
			rightLowerLeg.BodyType = BodyType.Dynamic;
			rightLowerLeg.CollisionCategories = collisionCat;
			rightLowerLeg.CollidesWith = Category.All & ~collisionCat;
			rightLowerLeg.Restitution = 0.15f;
			rightLowerLeg.Friction = friction;
			health.Add(rightLowerLeg, 1.0f);
			forceNextPos.Add(rightLowerLeg, MIN_POSE_TIME);
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

			r_neck = JointFactory.CreateRevoluteJoint(world, head, torso, -Vector2.UnitY * 20 * scale * MainGame.PIXEL_TO_METER);
			neck = JointFactory.CreateAngleJoint(world, head, torso);
			neck.CollideConnected = false;
			neck.MaxImpulse = maxImpulse;

			r_leftShoulder = JointFactory.CreateRevoluteJoint(world, leftUpperArm, torso, -Vector2.UnitY * 15 * scale * MainGame.PIXEL_TO_METER);
			leftShoulder = JointFactory.CreateAngleJoint(world, leftUpperArm, torso);
			leftShoulder.CollideConnected = false;
			leftShoulder.MaxImpulse = maxImpulse;

			r_rightShoulder = JointFactory.CreateRevoluteJoint(world, rightUpperArm, torso, -Vector2.UnitY * 15 * scale * MainGame.PIXEL_TO_METER);
			rightShoulder = JointFactory.CreateAngleJoint(world, rightUpperArm, torso);
			rightShoulder.CollideConnected = false;
			rightShoulder.MaxImpulse = maxImpulse;

			r_leftElbow = JointFactory.CreateRevoluteJoint(world, leftLowerArm, leftUpperArm, -Vector2.UnitY * 7.5f * scale * MainGame.PIXEL_TO_METER);
			leftElbow = JointFactory.CreateAngleJoint(world, leftLowerArm, leftUpperArm);
			leftElbow.CollideConnected = false;
			leftElbow.MaxImpulse = maxImpulse;

			r_rightElbow = JointFactory.CreateRevoluteJoint(world, rightLowerArm, rightUpperArm, -Vector2.UnitY * 7.5f * scale * MainGame.PIXEL_TO_METER);
			rightElbow = JointFactory.CreateAngleJoint(world, rightLowerArm, rightUpperArm);
			rightElbow.CollideConnected = false;
			rightElbow.MaxImpulse = maxImpulse;

			r_leftHip = JointFactory.CreateRevoluteJoint(world, leftUpperLeg, torso, Vector2.UnitY * 15 * scale * MainGame.PIXEL_TO_METER);
			leftHip = JointFactory.CreateAngleJoint(world, leftUpperLeg, torso);
			leftHip.CollideConnected = false;
			leftHip.MaxImpulse = maxImpulse;

			r_rightHip = JointFactory.CreateRevoluteJoint(world, rightUpperLeg, torso, Vector2.UnitY * 15 * scale * MainGame.PIXEL_TO_METER);
			rightHip = JointFactory.CreateAngleJoint(world, rightUpperLeg, torso);
			rightHip.CollideConnected = false;
			rightHip.MaxImpulse = maxImpulse;

			r_leftKnee = JointFactory.CreateRevoluteJoint(world, leftLowerLeg, leftUpperLeg, -Vector2.UnitY * 7.5f * scale * MainGame.PIXEL_TO_METER);
			leftKnee = JointFactory.CreateAngleJoint(world, leftUpperLeg, leftLowerLeg);
			leftKnee.CollideConnected = false;
			leftKnee.MaxImpulse = maxImpulse;

			r_rightKnee = JointFactory.CreateRevoluteJoint(world, rightLowerLeg, rightUpperLeg, -Vector2.UnitY * 7.5f * scale * MainGame.PIXEL_TO_METER);
			rightKnee = JointFactory.CreateAngleJoint(world, rightUpperLeg, rightLowerLeg);
			rightKnee.CollideConnected = false;
			rightKnee.MaxImpulse = maxImpulse;
		}

		/// <summary>
		/// Removes all limbs and joints from the physics world. Call this before respawning
		/// </summary>
		public void Destroy()
		{
			// Remove attacks
			for (int i = 0; i < attacks.Count; i++)
			{
				if (world.BodyList.Contains(attacks[i].PhysicsBody))
					world.RemoveBody(attacks[i].PhysicsBody);
			}
			attacks.Clear();

			// Remove joints
			List<Body> keys = health.Keys.ToList<Body>();
			foreach (Body b in keys)
				health[b] = 0f;

			// Remove limbs
			if (world.BodyList.Contains(head))
				world.RemoveBody(head);
			if (world.BodyList.Contains(torso))
				world.RemoveBody(torso);
			if (world.BodyList.Contains(leftUpperArm))
				world.RemoveBody(leftUpperArm);
			if (world.BodyList.Contains(leftLowerArm))
				world.RemoveBody(leftLowerArm);
			if (world.BodyList.Contains(rightUpperArm))
				world.RemoveBody(rightUpperArm);
			if (world.BodyList.Contains(rightLowerArm))
				world.RemoveBody(rightLowerArm);
			if (world.BodyList.Contains(leftUpperLeg))
				world.RemoveBody(leftUpperLeg);
			if (world.BodyList.Contains(leftLowerLeg))
				world.RemoveBody(leftLowerLeg);
			if (world.BodyList.Contains(rightUpperLeg))
				world.RemoveBody(rightUpperLeg);
			if (world.BodyList.Contains(rightLowerLeg))
				world.RemoveBody(rightLowerLeg);
			if (world.BodyList.Contains(gyro))
				world.RemoveBody(gyro);
		}

		#endregion

		#region Collision handlers

		/// <summary>
		/// Deals damage to limbs that collide with attacks
		/// </summary>
		/// <param name="fixtureA">The limb being damaged</param>
		/// <param name="fixtureB">The attack</param>
		/// <param name="contact">The contact between the two objects</param>
		/// <returns></returns>
		private bool SpecialCollisions(Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			if (fixtureB.Body.UserData is ForceWave)
			{
				ForceWave f = (ForceWave)fixtureB.Body.UserData;
				fixtureB.Body.UserData = null;

				health[fixtureA.Body] -= f.Damage / limbDefense;

				Random r = new Random();
				int index = r.Next(MainGame.sfx_punches.Length);
				MainGame.sfx_punches[index].Play();
			}
			else if (fixtureB.Body.UserData is LongRangeAttack)
			{
				LongRangeAttack f = (LongRangeAttack)fixtureB.Body.UserData;
				fixtureB.Body.UserData = null;

				health[fixtureA.Body] -= f.Damage / limbDefense;

				// TODO: Play sound
			}
			else if (fixtureB.Body.UserData is Trap)
			{
				Trap t = (Trap)fixtureB.Body.UserData;
				if (t.Open)
					t.Explode();
			}
			else if (fixtureB.Body.UserData is ExplosionParticle)
			{
				fixtureB.Body.CollidesWith = Category.None;
				health[fixtureA.Body] -= ((ExplosionParticle)fixtureB.Body.UserData).Damage / limbDefense;
			}
			else if (fixtureB.Body.UserData is TrapAmmo)
			{
				((TrapAmmo)fixtureB.Body.UserData).PickUp();
				this.trapAmmo = MAX_AMMO;
			}

			return contact.IsTouching;
		}

		#endregion

		#region Actions

		/// <summary>
		/// Sends the stick figure to its default pose
		/// </summary>
		public void Stand()
		{
			Crouching = false;
			leftLegLeft = true;
			rightLegLeft = false;
			upright.TargetAngle = 0.0f;
			walkStage = 0;
			if (!kicking || kicking && !kickLeg)
			{
				leftHip.TargetAngle = 3 * MathHelper.PiOver4;
				leftKnee.TargetAngle = -5 * MathHelper.PiOver4;
				leftLowerLeg.Friction = 0f;
			}
			if (!kicking || kicking && kickLeg)
			{
				rightHip.TargetAngle = -3 * MathHelper.PiOver4;
				rightKnee.TargetAngle = -3 * MathHelper.PiOver4;
				rightLowerLeg.Friction = 0f;
			}
			List<AngleJoint> checkThese = new List<AngleJoint>();
			if (health[leftUpperLeg] > 0f)
				checkThese.Add(leftHip);
			if (health[leftLowerLeg] > 0f)
				checkThese.Add(leftKnee);
			if (health[rightUpperLeg] > 0f)
				checkThese.Add(rightHip);
			if (health[rightLowerLeg] > 0f)
				checkThese.Add(rightKnee);
			if (JointsAreInPosition(checkThese))
			{
				leftLowerLeg.Friction = friction;
				rightLowerLeg.Friction = friction;
			}

			// Fixes friction not working
			if (OnGround)
			{
				if (Math.Abs(torso.LinearVelocity.X) > 0.05)
					torso.ApplyForce(Vector2.UnitX * -Math.Sign(torso.LinearVelocity.X) * scale * 150f);
				else
					torso.LinearVelocity = new Vector2(0f, torso.LinearVelocity.Y);
			}
		}

		/// <summary>
		/// Makes figure walk the the right (place in Update method)
		/// </summary>
		public void WalkRight()
		{
			if (kicking || IsDead || LockControl)
				return;

			leftLegLeft = false;
			rightLegLeft = false;
			LastFacedLeft = false;
			upright.TargetAngle = -0.1f;
			if (torso.LinearVelocity.X < (OnGround ? 4 : 3) && !(Crouching && OnGround))
				torso.ApplyForce(new Vector2(150, 0) * maxImpulse * (float)Math.Pow(scale, 1.5));
			List<AngleJoint> checkThese = new List<AngleJoint>();
			if (health[leftUpperLeg] > 0f)
				checkThese.Add(leftHip);
			if (health[rightUpperLeg] > 0f)
				checkThese.Add(rightHip);
			if (walkStage == 0)
			{
				leftHip.TargetAngle = (float)Math.PI - torso.Rotation;
				leftKnee.TargetAngle = -MathHelper.PiOver2 - torso.Rotation;
				rightHip.TargetAngle = -3 * MathHelper.PiOver4 - torso.Rotation;
				rightKnee.TargetAngle = -3 * MathHelper.PiOver4 - torso.Rotation;
				rightKnee.MaxImpulse = maxImpulse * 3 * scale * health[rightLowerLeg] * health[rightUpperLeg];
				leftLowerLeg.Friction = 0f;
				rightLowerLeg.Friction = friction;
				if (JointsAreInPosition(checkThese))
					walkStage = 1;
			}
			else if (walkStage == 1)
			{
				leftHip.TargetAngle = 3 * MathHelper.PiOver2 - torso.Rotation;
				leftKnee.TargetAngle = -MathHelper.PiOver2 - torso.Rotation;
				rightHip.TargetAngle = -5 * MathHelper.PiOver4 - torso.Rotation;
				rightKnee.TargetAngle = -(float)Math.PI - torso.Rotation;
				rightKnee.MaxImpulse = maxImpulse * scale * health[rightLowerLeg] * health[rightUpperLeg];
				if (JointsAreInPosition(checkThese))
					walkStage = 2;
			}
			else if (walkStage == 2)
			{
				leftHip.TargetAngle = 5 * MathHelper.PiOver4 - torso.Rotation;
				leftKnee.TargetAngle = -3 * MathHelper.PiOver4 - torso.Rotation;
				leftKnee.MaxImpulse = maxImpulse * 3 * scale * health[leftLowerLeg] * health[leftUpperLeg];
				rightHip.TargetAngle = -(float)Math.PI - torso.Rotation;
				rightKnee.TargetAngle = -MathHelper.PiOver2 - torso.Rotation;
				rightLowerLeg.Friction = 0f;
				leftLowerLeg.Friction = friction;
				if (JointsAreInPosition(checkThese))
					walkStage = 3;
			}
			else if (walkStage == 3)
			{
				leftHip.TargetAngle = 3 * MathHelper.PiOver4 - torso.Rotation;
				leftKnee.TargetAngle = -(float)Math.PI - torso.Rotation;
				leftKnee.MaxImpulse = maxImpulse * scale * health[leftLowerLeg] * health[leftUpperLeg];
				rightHip.TargetAngle = -MathHelper.PiOver2 - torso.Rotation;
				rightKnee.TargetAngle = -MathHelper.PiOver2 - torso.Rotation;
				if (JointsAreInPosition(checkThese))
					walkStage = 0;
			}
		}
	
		/// <summary>
		/// Makes figure walk to the left (place in Update method)
		/// </summary>
		public void WalkLeft()
		{
			if (kicking || IsDead || LockControl)
				return;

			LastFacedLeft = true;
			leftLegLeft = true;
			rightLegLeft = true;
			upright.TargetAngle = 0.1f;
			if (torso.LinearVelocity.X > (OnGround ? -4 : -3))
				torso.ApplyForce(new Vector2(-150, 0) * maxImpulse * (float)Math.Pow(scale, 1.5));
			List<AngleJoint> checkThese = new List<AngleJoint>();
			if (health[leftUpperLeg] > 0f)
				checkThese.Add(leftHip);
			if (health[rightUpperLeg] > 0f)
				checkThese.Add(rightHip);
			if (walkStage == 0)
			{
				rightHip.TargetAngle = -(float)Math.PI - torso.Rotation;
				rightKnee.TargetAngle = -3 * MathHelper.PiOver2 - torso.Rotation;
				leftHip.TargetAngle = 3 * MathHelper.PiOver4 - torso.Rotation;
				leftKnee.TargetAngle = -5 * MathHelper.PiOver4 - torso.Rotation;
				leftKnee.MaxImpulse = maxImpulse * scale * 3 * health[leftLowerLeg] * health[leftUpperLeg];
				rightLowerLeg.Friction = 0f;
				leftLowerLeg.Friction = friction;
				if (JointsAreInPosition(checkThese))
					walkStage = 1;
			}
			else if (walkStage == 1)
			{
				rightHip.TargetAngle = -3 * MathHelper.PiOver2 - torso.Rotation;
				rightKnee.TargetAngle = -3 * MathHelper.PiOver2 - torso.Rotation;
				leftHip.TargetAngle = 5 * MathHelper.PiOver4 - torso.Rotation;
				leftKnee.TargetAngle = -(float)Math.PI - torso.Rotation;
				leftKnee.MaxImpulse = maxImpulse * scale * health[leftLowerLeg] * health[leftUpperLeg];
				if (JointsAreInPosition(checkThese))
					walkStage = 2;
			}
			else if (walkStage == 2)
			{
				rightHip.TargetAngle = -5 * MathHelper.PiOver4 - torso.Rotation;
				rightKnee.TargetAngle = -5 * MathHelper.PiOver4 - torso.Rotation;
				rightKnee.MaxImpulse = maxImpulse * scale * 3 * health[rightLowerLeg] * health[rightUpperLeg];
				leftHip.TargetAngle = (float)Math.PI - torso.Rotation;
				leftKnee.TargetAngle = -3 * MathHelper.PiOver2 - torso.Rotation;
				leftLowerLeg.Friction = 0f;
				rightLowerLeg.Friction = friction;
				if (JointsAreInPosition(checkThese))
					walkStage = 3;
			}
			else if (walkStage == 3)
			{
				rightHip.TargetAngle = -3 * MathHelper.PiOver4 - torso.Rotation;
				rightKnee.TargetAngle = -(float)Math.PI - torso.Rotation;
				rightKnee.MaxImpulse = maxImpulse * scale * health[rightLowerLeg] * health[rightUpperLeg];
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
			if (IsDead || LockControl)
				return;

			leftLegLeft = true;
			rightLegLeft = false;
			upright.TargetAngle = 0.0f;
			if (!kicking || kicking && !kickLeg)
			{
				leftHip.TargetAngle = MathHelper.Pi;
				leftKnee.TargetAngle = -MathHelper.Pi;
			}
			if (!kicking || kicking && kickLeg)
			{
				rightHip.TargetAngle = -MathHelper.Pi;
				rightKnee.TargetAngle = -MathHelper.Pi;
			}
			if (OnGround)
			{
				leftLowerLeg.Friction = friction;
				rightLowerLeg.Friction = friction;
				int deadLimbs = 0;
				if (health[leftLowerLeg] <= 0)
					deadLimbs++;
				if (health[leftUpperLeg] <= 0)
					deadLimbs++;
				if (health[rightLowerLeg] <= 0)
					deadLimbs++;
				if (health[rightUpperLeg] <= 0)
					deadLimbs++;

				float force = -4f;
				torso.LinearVelocity = new Vector2(torso.LinearVelocity.X, force * (float)Math.Pow(scale, 2.5));
				head.LinearVelocity = new Vector2(head.LinearVelocity.X, force * (float)Math.Pow(scale, 2.5));
				if (health[leftUpperArm] > 0f)
					leftUpperArm.LinearVelocity = new Vector2(leftUpperArm.LinearVelocity.X, force * (float)Math.Pow(scale, 2.5));
				if (health[rightUpperArm] > 0f)				
					rightUpperArm.LinearVelocity = new Vector2(rightUpperArm.LinearVelocity.X, force * (float)Math.Pow(scale, 2.5));
				if (health[leftLowerArm] > 0f)
					leftLowerArm.LinearVelocity = new Vector2(leftLowerArm.LinearVelocity.X, force * (float)Math.Pow(scale, 2.5));
				if (health[rightLowerArm] > 0f)
					rightLowerArm.LinearVelocity = new Vector2(rightLowerArm.LinearVelocity.X, force * (float)Math.Pow(scale, 2.5));
				if (health[leftUpperLeg] > 0f)
					leftUpperLeg.LinearVelocity = new Vector2(leftUpperLeg.LinearVelocity.X, force * (float)Math.Pow(scale, 2.5));
				if (health[rightUpperLeg] > 0f)
					rightUpperLeg.LinearVelocity = new Vector2(rightUpperLeg.LinearVelocity.X, force * (float)Math.Pow(scale, 2.5));
				if (health[leftLowerLeg] > 0f)
					leftLowerLeg.LinearVelocity = new Vector2(leftLowerLeg.LinearVelocity.X, force * (float)Math.Pow(scale, 2.5));
				if (health[rightLowerLeg] > 0f)
					rightLowerLeg.LinearVelocity = new Vector2(rightLowerLeg.LinearVelocity.X, force * (float)Math.Pow(scale, 2.5));
			}
			Crouching = false;
		}

		/// <summary>
		/// Makes the figure crouch
		/// </summary>
		public void Crouch()
		{
			if (IsDead || LockControl)
				return;

			leftLegLeft = true;
			rightLegLeft = false;
			upright.TargetAngle = 0.0f;
			if (!kicking || kicking && !kickLeg)
			{
				leftLowerLeg.Friction = friction;
				leftHip.TargetAngle = MathHelper.PiOver4;
				leftKnee.TargetAngle = -7 * MathHelper.PiOver4;
			}
			if (!kicking || kicking && kickLeg)
			{
				rightLowerLeg.Friction = friction;
				rightHip.TargetAngle = -MathHelper.PiOver4;
				rightKnee.TargetAngle = -MathHelper.PiOver4;
			}

			// Fixes friction not working
			if (OnGround)
			{
				if (Math.Abs(torso.LinearVelocity.X) > 0.05)
					torso.ApplyForce(Vector2.UnitX * -Math.Sign(torso.LinearVelocity.X) * scale * 150f);
				else
					torso.LinearVelocity = new Vector2(0f, torso.LinearVelocity.Y);
			}
		}

		/// <summary>
		/// Punches
		/// </summary>
		/// <param name="angle">The angle at which to punch</param>
		public void Punch(float angle)
		{
			if (!IsDead && !LockControl)
			{
				punching = true;
				attackAngle = angle;
			}
		}

		/// <summary>
		/// Kicks
		/// </summary>
		/// <param name="angle">The angle at which to kick</param>
		public void Kick(float angle)
		{
			if (!IsDead && (health[leftUpperLeg] > 0f || health[rightUpperLeg] > 0f) && !LockControl)
			{
				kicking = true;
				kickLeg = (angle > MathHelper.PiOver2 || angle < -MathHelper.PiOver2) && health[leftUpperLeg] > 0f || health[rightUpperLeg] <= 0f;
				attackAngle = angle;
			}
		}

		/// <summary>
		/// Readys the figure's long range attack
		/// </summary>
		/// <param name="angle">The angle to aim at</param>
		public void Aim(float angle)
		{
			if (!IsDead && AllowLongRange && !LockControl)
			{
				aiming = true;
				attackAngle = angle;

				if (chargeUp < MAX_CHARGE)
					chargeUp++;

				// TODO: Start charge up sound
			}
		}

		/// <summary>
		/// Executes a long range attack
		/// </summary>
		public void LongRangeAttack()
		{
			aiming = false;
			if (IsDead || health[leftLowerArm] <= 0f && health[rightLowerArm] <= 0f || !AllowLongRange || LockControl)
				return;

			if (coolDown <= 0)
			{
				attacks.Add(new LongRangeAttack(world, health[leftLowerArm] > 0f ? LeftHandPosition : RightHandPosition, (-Vector2.UnitX * (float)Math.Sin(attackAngle - MathHelper.PiOver2) - Vector2.UnitY * (float)Math.Cos(attackAngle - MathHelper.PiOver2)) * (15f + chargeUp / 15f), 0.1f + 0.2f * (chargeUp / MAX_CHARGE), collisionCat));
				ApplyForce((-Vector2.UnitX * (float)Math.Sin(attackAngle - MathHelper.PiOver2) - Vector2.UnitY * (float)Math.Cos(attackAngle - MathHelper.PiOver2)) * (20f + chargeUp / 20f) * -5f * scale);
				chargeUp = 0;
				coolDown = COOL_PERIOD;
			}

			// TODO: End charge up sound, play shoot sound
		}

		/// <summary>
		/// Throws a trap
		/// </summary>
		public void ThrowTrap(float angle)
		{
			if (!IsDead && trapAmmo > 0 && trapThrowTime <= 0 && AllowTraps && !LockControl)
			{
				trapAmmo--;
				trapThrowTime = THROW_TIME;
				throwing = true;
				throwArm = angle > MathHelper.PiOver2 || angle < -MathHelper.PiOver2;
				attackAngle = angle;
			}
		}

		#endregion

		#region Updating

		/// <summary>
		/// Updates some of the stick figures' key stances
		/// </summary>
		public virtual void Update()
		{
			UpdateArms();
			if (kicking)
				UpdateKicks();

			List<Attack> toRemove = new List<Attack>();
			foreach (Attack a in attacks)
			{
				a.Update();
				if (a.PhysicsBody.UserData == null)
					toRemove.Add(a);
			}
			foreach (Attack a in toRemove)
			{
				if (world.BodyList.Contains(a.PhysicsBody))
					world.RemoveBody(a.PhysicsBody);
				attacks.Remove(a);
			}
			if (coolDown > 0)
				coolDown--;
			if (trapThrowTime > 0)
				trapThrowTime--;
			if (!AllowTraps)
				trapAmmo = 0;

			UpdateLimbStrength();
			UpdateLimbAttachment();
			UpdateLimbFriction();

			if (Jumping)
				Jump();
			else if (increaseFall)
			{
				if (torso.LinearVelocity.Y < -2)
					torso.ApplyForce(Vector2.UnitY * 250);
				else
					increaseFall = false;
			}
			if (Crouching)
				Crouch();
		}

		/// <summary>
		/// Orients arms in necessary position
		/// </summary>
		private void UpdateArms()
		{
			if (!punching && !aiming && !throwing)
			{
				leftShoulder.TargetAngle = 3 * MathHelper.PiOver4;
				rightShoulder.TargetAngle = -3 * MathHelper.PiOver4;
				leftElbow.TargetAngle = MathHelper.PiOver4;
				rightElbow.TargetAngle = -MathHelper.PiOver4;
			}
			else if (aiming)
			{
				leftShoulder.TargetAngle = GetArmTargetAngle(attackAngle - MathHelper.PiOver2, true);
				rightShoulder.TargetAngle = GetArmTargetAngle(attackAngle - MathHelper.PiOver2, false);
				leftElbow.TargetAngle = 0f;
				rightElbow.TargetAngle = 0f;
			}
			else if (punching || throwing)
			{
				if (punchArm && health[leftLowerArm] <= 0f || !punchArm && health[rightLowerArm] <= 0f)
					punchArm = !punchArm;
				if (throwArm && health[leftLowerArm] <= 0f || !throwArm && health[rightLowerArm] <= 0f)
					throwArm = !throwArm;

				List<AngleJoint> checkThese = new List<AngleJoint>();
				if ((punching && punchArm) || (throwing && throwArm) && health[leftLowerArm] > 0f)
					checkThese.Add(leftShoulder);
				if (!(punching && punchArm) || (throwing && throwArm) && health[rightLowerArm] > 0f)
					checkThese.Add(rightShoulder);
				if (checkThese.Count == 0)
					return;

				if (punchStage == -1)
				{
					MainGame.sfx_whoosh.Play();
					punchStage = 0;
					float angle = attackAngle - MathHelper.PiOver2;
					if (punching && punchArm || throwing && throwArm)
					{
						leftShoulder.TargetAngle = GetArmTargetAngle(angle, true);
						leftElbow.TargetAngle = 0f;
						leftShoulder.MaxImpulse = 1000f * scale;
						leftElbow.MaxImpulse = 1000f * scale;
						leftUpperArm.CollidesWith = Category.Cat31;
						leftLowerArm.CollidesWith = Category.Cat31;
					}
					else
					{
						rightShoulder.TargetAngle = GetArmTargetAngle(angle, false);
						rightElbow.TargetAngle = 0f;
						rightShoulder.MaxImpulse = 1000f * scale;
						rightElbow.MaxImpulse = 1000f * scale;
						rightUpperArm.CollidesWith = Category.Cat31;
						rightLowerArm.CollidesWith = Category.Cat31;
					}
				}
				else if (punchStage == 0)
				{
					if (JointsAreInPosition(checkThese))
					{
						float angle = attackAngle - MathHelper.PiOver2;
						if (punching && punchArm || throwing && throwArm)
						{
							leftShoulder.TargetAngle = 3 * MathHelper.PiOver4;
							leftElbow.TargetAngle = MathHelper.PiOver4;
							leftShoulder.MaxImpulse = maxImpulse * scale;
							leftElbow.MaxImpulse = maxImpulse * scale;
							if (punching)
								attacks.Add(new ForceWave(world, LeftHandPosition, new Vector2(-(float)Math.Sin(angle), -(float)Math.Cos(angle)) * 10, this.collisionCat));
							else if (health[leftLowerArm] > 0f)
								attacks.Add(new Trap(world, LeftHandPosition, new Vector2(-(float)Math.Sin(angle), -(float)Math.Cos(angle)) * 10, this.collisionCat));
						}
						else
						{
							rightShoulder.TargetAngle = -3 * MathHelper.PiOver4;
							rightElbow.TargetAngle = -MathHelper.PiOver4;
							rightShoulder.MaxImpulse = maxImpulse * scale;
							rightElbow.MaxImpulse = maxImpulse * scale;
							if (punching)
								attacks.Add(new ForceWave(world, RightHandPosition, new Vector2(-(float)Math.Sin(angle), -(float)Math.Cos(angle)) * 10, this.collisionCat));
							else if (health[rightLowerArm] > 0f)
								attacks.Add(new Trap(world, RightHandPosition, new Vector2(-(float)Math.Sin(angle), -(float)Math.Cos(angle)) * 10, this.collisionCat));
						}
						punchStage = 1;
					}
				}
				else if (punchStage == 1)
				{
					if (JointsAreInPosition(checkThese))
					{
						if (punchArm && health[rightUpperArm] > 0f || !punchArm && health[leftUpperArm] > 0f)
							punchArm = !punchArm;
						if (throwArm && health[leftLowerArm] <= 0f || !throwArm && health[rightLowerArm] <= 0f)
							throwArm = !throwArm;
						punching = false;
						throwing = false;
						punchStage = -1;
						leftUpperArm.CollidesWith = Category.All & ~this.collisionCat;
						leftLowerArm.CollidesWith = Category.All & ~this.collisionCat;
						rightUpperArm.CollidesWith = Category.All & ~this.collisionCat;
						rightLowerArm.CollidesWith = Category.All & ~this.collisionCat;
					}
				}
			}
		}

		/// <summary>
		/// Updates the kick animation
		/// </summary>
		private void UpdateKicks()
		{
			List<AngleJoint> checkThese = new List<AngleJoint>();
			if (kickLeg && health[leftUpperLeg] > 0f)
				checkThese.Add(leftHip);
			if (!kickLeg && health[rightUpperLeg] > 0f)
				checkThese.Add(rightHip);
			if (checkThese.Count == 0)
				return;

			if (kickStage == -1)
			{
				this.walkStage = 0;
				leftLowerLeg.Friction = 0f;
				rightLowerLeg.Friction = 0f;
				MainGame.sfx_whoosh.Play();
				kickStage = 0;
				float angle = attackAngle - MathHelper.PiOver2;
				if (kickLeg)
				{
					leftHip.TargetAngle = GetLegTargetAngle(angle, kickLeg);
					leftKnee.TargetAngle = -MathHelper.Pi;
					leftHip.MaxImpulse = 1000f * scale;
					leftKnee.MaxImpulse = 1000f * scale;
					leftLowerLeg.CollidesWith = Category.Cat31;
					leftUpperLeg.CollidesWith = Category.Cat31;
				}
				else
				{
					rightHip.TargetAngle = GetLegTargetAngle(angle, kickLeg);
					rightKnee.TargetAngle = -MathHelper.Pi;
					rightHip.MaxImpulse = 1000f * scale;
					rightKnee.MaxImpulse = 1000f * scale;
					rightLowerLeg.CollidesWith = Category.Cat31;
					rightUpperLeg.CollidesWith = Category.Cat31;
				}
			}
			else if (kickStage == 0)
			{
				if (JointsAreInPosition(checkThese))
				{
					float angle = attackAngle - MathHelper.PiOver2;
					if (kickLeg)
					{
						leftHip.TargetAngle = 3 * MathHelper.PiOver4;
						leftKnee.TargetAngle = -5 * MathHelper.PiOver4;
						leftHip.MaxImpulse = maxImpulse * scale;
						leftKnee.MaxImpulse = maxImpulse * scale;
						attacks.Add(new ForceWave(world, LeftFootPosition, new Vector2(-(float)Math.Sin(angle), -(float)Math.Cos(angle)) * 10, this.collisionCat));
					}
					else
					{
						rightHip.TargetAngle = -3 * MathHelper.PiOver4;
						rightKnee.TargetAngle = -3 * MathHelper.PiOver4;
						rightHip.MaxImpulse = maxImpulse * scale;
						rightKnee.MaxImpulse = maxImpulse * scale;
						attacks.Add(new ForceWave(world, RightFootPosition, new Vector2(-(float)Math.Sin(angle), -(float)Math.Cos(angle)) * 10, this.collisionCat));
					}
					kickStage = 1;
				}
			}
			else if (kickStage == 1)
			{
				if (JointsAreInPosition(checkThese))
				{
					kickLeg = !kickLeg;
					kicking = false;
					kickStage = -1;
					leftLowerLeg.CollidesWith = Category.All & ~this.collisionCat;
					leftUpperLeg.CollidesWith = Category.All & ~this.collisionCat;
					rightLowerLeg.CollidesWith = Category.All & ~this.collisionCat;
					rightUpperLeg.CollidesWith = Category.All & ~this.collisionCat;
				}
			}
		}

		/// <summary>
		/// Detaches limbs that lose all their health
		/// </summary>
		private void UpdateLimbAttachment()
		{
			// Left arm
			if (health[leftUpperArm] <= 0)
			{
				health[leftLowerArm] = 0f;
				leftUpperArm.Friction = 3.0f;
				if (world.JointList.Contains(leftShoulder))
					world.RemoveJoint(leftShoulder);
				if (world.JointList.Contains(r_leftShoulder))
					world.RemoveJoint(r_leftShoulder);
				torso.OnCollision += new OnCollisionEventHandler(SpecialCollisions);
			}
			if (health[leftLowerArm] <= 0)
			{
				leftLowerArm.Friction = 3.0f;
				if (world.JointList.Contains(leftElbow))
					world.RemoveJoint(leftElbow);
				if (world.JointList.Contains(r_leftElbow))
					world.RemoveJoint(r_leftElbow);
			}

			// Right arm
			if (health[rightUpperArm] <= 0)
			{
				health[rightLowerArm] = 0f;
				rightUpperArm.Friction = 3.0f;
				if (world.JointList.Contains(rightShoulder))
					world.RemoveJoint(rightShoulder);
				if (world.JointList.Contains(r_rightShoulder))
					world.RemoveJoint(r_rightShoulder);
				torso.OnCollision += new OnCollisionEventHandler(SpecialCollisions);
			}
			if (health[rightLowerArm] <= 0)
			{
				rightLowerArm.Friction = 3.0f;
				if (world.JointList.Contains(rightElbow))
					world.RemoveJoint(rightElbow);
				if (world.JointList.Contains(r_rightElbow))
					world.RemoveJoint(r_rightElbow);
			}

			// Left leg
			if (health[leftUpperLeg] <= 0)
			{
				health[leftLowerLeg] = 0f;
				leftUpperLeg.Friction = 3.0f;
				if (world.JointList.Contains(leftHip))
					world.RemoveJoint(leftHip);
				if (world.JointList.Contains(r_leftHip))
					world.RemoveJoint(r_leftHip);
				torso.OnCollision += new OnCollisionEventHandler(SpecialCollisions);
			}
			if (health[leftLowerLeg] <= 0)
			{
				leftLowerLeg.Friction = 3.0f;
				if (world.JointList.Contains(leftKnee))
					world.RemoveJoint(leftKnee);
				if (world.JointList.Contains(r_leftKnee))
					world.RemoveJoint(r_leftKnee);
			}

			// Right leg
			if (health[rightUpperLeg] <= 0)
			{
				health[rightLowerLeg] = 0f;
				rightUpperLeg.Friction = 3.0f;
				if (world.JointList.Contains(rightHip))
					world.RemoveJoint(rightHip);
				if (world.JointList.Contains(r_rightHip))
					world.RemoveJoint(r_rightHip);
				torso.OnCollision += new OnCollisionEventHandler(SpecialCollisions);
			}
			if (health[rightLowerLeg] <= 0)
			{
				rightLowerLeg.Friction = 3.0f;
				if (world.JointList.Contains(rightKnee))
					world.RemoveJoint(rightKnee);
				if (world.JointList.Contains(r_rightKnee))
					world.RemoveJoint(r_rightKnee);
			}

			// Torso
			if (health[torso] <= 0)
			{
				torso.Friction = 3.0f;
				if (world.JointList.Contains(upright))
					world.RemoveJoint(upright);
			}

			// Head
			if (health[head] <= 0)
			{
				head.Friction = 3.0f;
				if (world.JointList.Contains(neck))
					world.RemoveJoint(neck);
				if (world.JointList.Contains(r_neck))
					world.RemoveJoint(r_neck);
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
			upright.MaxImpulse = maxImpulse * health[torso] * health[head] * scale;
			neck.MaxImpulse = maxImpulse * health[head] * health[torso] * scale;
			leftShoulder.MaxImpulse = maxImpulse * health[torso] * health[leftUpperArm] * health[head] * scale;
			leftElbow.MaxImpulse = maxImpulse * health[leftUpperArm] * health[leftLowerArm] * health[torso] * health[head] * scale;
			rightShoulder.MaxImpulse = maxImpulse * health[torso] * health[rightUpperArm] * health[head] * scale;
			rightElbow.MaxImpulse = maxImpulse * health[rightUpperArm] * health[rightLowerArm] * health[torso] * health[head] * scale;
			leftHip.MaxImpulse = maxImpulse * health[torso] * health[leftUpperLeg] * health[head] * scale;
			leftKnee.MaxImpulse = maxImpulse * health[leftUpperLeg] * health[leftLowerLeg] * health[torso] * health[head] * scale;
			rightHip.MaxImpulse = maxImpulse * health[torso] * health[rightUpperLeg] * health[head] * scale;
			rightKnee.MaxImpulse = maxImpulse * health[rightUpperLeg] * health[rightLowerLeg] * health[torso] * health[head] * scale;
		}

		/// <summary>
		/// Prevents the Stick Figure from sticking to walls
		/// Lol, "stick" figure.
		/// </summary>
		private void UpdateLimbFriction()
		{
			head.Friction = OnGround || health[head] <= 0 || !(health[leftLowerLeg] <= 0 && health[rightLowerLeg] <= 0 && health[leftUpperLeg] <= 0 && health[rightUpperLeg] <= 0) ? friction : 0f;
			torso.Friction = OnGround || health[torso] <= 0 ? friction : 0f;
			leftUpperArm.Friction = OnGround || health[leftUpperArm] <= 0 ? friction : 0f;
			leftLowerArm.Friction = OnGround || health[leftLowerArm] <= 0 ? friction : 0f;
			rightUpperArm.Friction = OnGround || health[rightUpperArm] <= 0 ? friction : 0f;
			rightLowerArm.Friction = OnGround || health[rightLowerArm] <= 0 ? friction : 0f;
			leftUpperLeg.Friction = OnGround || health[leftUpperLeg] <= 0 ? friction : 0f;
			rightUpperLeg.Friction = OnGround || health[rightUpperLeg] <= 0 ? friction : 0f;
			if (health[leftLowerLeg] <= 0)
				leftLowerLeg.Friction = friction;
			if (health[rightLowerLeg] <= 0)
				rightLowerLeg.Friction = friction;
		}

		#endregion

		#region Helpers/debug

		/// <summary>
		/// Checks if all the joints in a list are close to their target angle
		/// </summary>
		/// <param name="joints">The array of joints to check</param>
		/// <returns>True if the joints are at their target angles, false if not</returns>
		private bool JointsAreInPosition(List<AngleJoint> joints)
		{
			List<Body> bodyCheck = new List<Body>();
			foreach (AngleJoint j in joints)
			{
				if (Math.Abs(j.BodyB.Rotation - j.BodyA.Rotation - j.TargetAngle) > 0.20)
				{
					if (!bodyCheck.Contains(j.BodyA))
					{
						bodyCheck.Add(j.BodyA);
						forceNextPos[j.BodyA]--;
					}
					if (!bodyCheck.Contains(j.BodyB))
					{
						bodyCheck.Add(j.BodyB);
						forceNextPos[j.BodyB]--;
					}
				}
			}

			bool ret = true;
			if (bodyCheck.Count > 0)
			{
				foreach (Body b in bodyCheck)
				{
					if (forceNextPos[b] > 0)
						ret = false;
				}
			}
			if (ret == true)
				foreach (Body b in bodyCheck)
					forceNextPos[b] = MIN_POSE_TIME;
			
			return ret;
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

		/// <summary>
		/// Takes an angle and converts it to an angle suitable for punching
		/// </summary>
		/// <param name="physAngle">The original angle</param>
		/// <param name="leftArm">Which arm is doing the punching</param>
		/// <returns></returns>
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

		/// <summary>
		/// Takes an angle and converts it to an angle suitable for kicking
		/// </summary>
		/// <param name="physAngle">The original angle</param>
		/// <param name="leftLeg">Which leg is kicking</param>
		/// <returns></returns>
		private float GetLegTargetAngle(float physAngle, bool leftLeg)
		{
			if (leftLeg)
			{
				if (physAngle > 0)
					return physAngle;
				else
					return physAngle + MathHelper.TwoPi;
			}
			else
				return physAngle;
		}

		/// <summary>
		/// Applies a force to the stick figure
		/// </summary>
		/// <param name="force">The force to apply</param>
		public void ApplyForce(Vector2 force)
		{
			if (health[head] > 0) head.ApplyForce(force * head.Mass);
			if (health[torso] > 0) torso.ApplyForce(force * torso.Mass);
			if (health[leftLowerArm] > 0) leftLowerArm.ApplyForce(force * leftLowerArm.Mass);
			if (health[leftUpperArm] > 0) leftUpperArm.ApplyForce(force * leftUpperArm.Mass);
			if (health[rightLowerArm] > 0) rightLowerArm.ApplyForce(force * rightLowerArm.Mass);
			if (health[rightUpperArm] > 0) rightUpperArm.ApplyForce(force * rightUpperArm.Mass);
			if (health[leftLowerLeg] > 0) leftLowerLeg.ApplyForce(force * leftLowerLeg.Mass);
			if (health[leftUpperLeg] > 0) leftUpperLeg.ApplyForce(force * leftUpperLeg.Mass);
			if (health[rightLowerLeg] > 0) rightLowerLeg.ApplyForce(force * rightLowerLeg.Mass);
			if (health[rightUpperLeg] > 0) rightUpperLeg.ApplyForce(force * rightUpperLeg.Mass);
		}

		#endregion

		#region Drawing

		/// <summary>
		/// Draws the stick figure
		/// </summary>
		/// <param name="sb">The SpriteBatch used to draw the stick figure</param>
		public virtual void Draw(SpriteBatch sb)
		{
			// More debug
//			sb.Draw(MainGame.tex_debugLimb, leftUpperArm.Position * MainGame.METER_TO_PIXEL, null, color, leftUpperArm.Rotation, new Vector2(MainGame.tex_debugLimb.Width / 2, MainGame.tex_debugLimb.Height / 2), scale, SpriteEffects.None, 0.0f);
//			sb.Draw(MainGame.tex_debugLimb, rightUpperArm.Position * MainGame.METER_TO_PIXEL, null, color, rightUpperArm.Rotation, new Vector2(MainGame.tex_debugLimb.Width / 2, MainGame.tex_debugLimb.Height / 2), scale, SpriteEffects.None, 0.0f);
//			sb.Draw(MainGame.tex_debugLimb, leftLowerArm.Position * MainGame.METER_TO_PIXEL, null, color, leftLowerArm.Rotation, new Vector2(MainGame.tex_debugLimb.Width / 2, MainGame.tex_debugLimb.Height / 2), scale, SpriteEffects.None, 0.0f);
//			sb.Draw(MainGame.tex_debugLimb, rightLowerArm.Position * MainGame.METER_TO_PIXEL, null, color, rightLowerArm.Rotation, new Vector2(MainGame.tex_debugLimb.Width / 2, MainGame.tex_debugLimb.Height / 2), scale, SpriteEffects.None, 0.0f);
//			sb.Draw(MainGame.tex_debugLimb, leftUpperLeg.Position * MainGame.METER_TO_PIXEL, null, color, leftUpperLeg.Rotation, new Vector2(MainGame.tex_debugLimb.Width / 2, MainGame.tex_debugLimb.Height / 2), scale, SpriteEffects.None, 0.0f);
//			sb.Draw(MainGame.tex_debugLimb, rightUpperLeg.Position * MainGame.METER_TO_PIXEL, null, color, rightUpperLeg.Rotation, new Vector2(MainGame.tex_debugLimb.Width / 2, MainGame.tex_debugLimb.Height / 2), scale, SpriteEffects.None, 0.0f);
//			sb.Draw(MainGame.tex_debugLimb, leftLowerLeg.Position * MainGame.METER_TO_PIXEL, null, color, leftLowerLeg.Rotation, new Vector2(MainGame.tex_debugLimb.Width / 2, MainGame.tex_debugLimb.Height / 2), scale, SpriteEffects.None, 0.0f);
//			sb.Draw(MainGame.tex_debugLimb, rightLowerLeg.Position * MainGame.METER_TO_PIXEL, null, color, rightLowerLeg.Rotation, new Vector2(MainGame.tex_debugLimb.Width / 2, MainGame.tex_debugLimb.Height / 2), scale, SpriteEffects.None, 0.0f);
//			sb.Draw(MainGame.tex_debugTorso, torso.Position * MainGame.METER_TO_PIXEL, null, color, torso.Rotation, new Vector2(MainGame.tex_debugTorso.Width / 2, MainGame.tex_debugTorso.Height / 2), scale, SpriteEffects.None, 0.0f);
//			sb.Draw(MainGame.tex_debugHead, head.Position * MainGame.METER_TO_PIXEL, null, color, head.Rotation, new Vector2(MainGame.tex_debugHead.Width / 2, MainGame.tex_debugHead.Height / 2), scale, SpriteEffects.None, 0.0f);

			Color deathColor = Color.Black;
			Color c = Blend(color, deathColor, health[leftUpperArm]);
			sb.Draw(evilSkin ? MainGame.tex_evil_armUpper : MainGame.tex_armUpper, leftUpperArm.Position * MainGame.METER_TO_PIXEL, null, c, leftUpperArm.Rotation, new Vector2(MainGame.tex_armUpper.Width / 2, MainGame.tex_armUpper.Height / 2), scale / 7f, LastFacedLeft || health[leftUpperArm] <= 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[rightUpperArm]);
			sb.Draw(evilSkin ? MainGame.tex_evil_armUpper : MainGame.tex_armUpper, rightUpperArm.Position * MainGame.METER_TO_PIXEL, null, c, rightUpperArm.Rotation, new Vector2(MainGame.tex_armUpper.Width / 2, MainGame.tex_armUpper.Height / 2), scale / 7f, LastFacedLeft || health[rightUpperArm] <= 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[leftUpperLeg]);
			sb.Draw(evilSkin ? MainGame.tex_evil_legUpper : MainGame.tex_legUpper, leftUpperLeg.Position * MainGame.METER_TO_PIXEL, null, c, leftUpperLeg.Rotation, new Vector2(MainGame.tex_legUpper.Width / 2, MainGame.tex_legUpper.Height / 2), scale / 7f, !leftLegLeft || health[leftUpperLeg] <= 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[rightUpperLeg]);
			sb.Draw(evilSkin ? MainGame.tex_evil_legUpper : MainGame.tex_legUpper, rightUpperLeg.Position * MainGame.METER_TO_PIXEL, null, c, rightUpperLeg.Rotation, new Vector2(MainGame.tex_legUpper.Width / 2, MainGame.tex_legUpper.Height / 2), scale / 7f, !rightLegLeft || health[rightUpperLeg] <= 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[leftLowerLeg]);
			sb.Draw(evilSkin ? MainGame.tex_evil_legLower : MainGame.tex_legLower, leftLowerLeg.Position * MainGame.METER_TO_PIXEL, null, c, leftLowerLeg.Rotation, new Vector2(MainGame.tex_legLower.Width / 2, MainGame.tex_legLower.Height / 2), scale / 7f, leftLegLeft || health[leftLowerLeg] <= 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[rightLowerLeg]);
			sb.Draw(evilSkin ? MainGame.tex_evil_legLower : MainGame.tex_legLower, rightLowerLeg.Position * MainGame.METER_TO_PIXEL, null, c, rightLowerLeg.Rotation, new Vector2(MainGame.tex_legLower.Width / 2, MainGame.tex_legLower.Height / 2), scale / 7f, rightLegLeft || health[rightLowerLeg] <= 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[torso]);
			sb.Draw(evilSkin ? MainGame.tex_evil_torso : MainGame.tex_torso, torso.Position * MainGame.METER_TO_PIXEL + Vector2.UnitY * 5, null, c, torso.Rotation, new Vector2(MainGame.tex_torso.Width / 2, MainGame.tex_torso.Height / 2), scale / 5f, LastFacedLeft || health[torso] <= 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[leftLowerArm]);
			sb.Draw(evilSkin ? MainGame.tex_evil_armLower : MainGame.tex_armLower, leftLowerArm.Position * MainGame.METER_TO_PIXEL - Vector2.UnitX * 2, null, c, leftLowerArm.Rotation + MathHelper.Pi, new Vector2(MainGame.tex_armLower.Width / 2, MainGame.tex_armLower.Height / 2), scale / 7f, LastFacedLeft || health[leftLowerArm] <= 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[rightLowerArm]);
			sb.Draw(evilSkin ? MainGame.tex_evil_armLower : MainGame.tex_armLower, rightLowerArm.Position * MainGame.METER_TO_PIXEL + Vector2.UnitX * 2, null, c, rightLowerArm.Rotation + MathHelper.Pi, new Vector2(MainGame.tex_armLower.Width / 2, MainGame.tex_armLower.Height / 2), scale / 7f, LastFacedLeft || health[rightUpperArm] <= 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
			c = Blend(color, deathColor, health[head]);
			sb.Draw(evilSkin ? MainGame.tex_evil_head : MainGame.tex_head, head.Position * MainGame.METER_TO_PIXEL, null, c, head.Rotation, new Vector2(MainGame.tex_head.Width / 2, MainGame.tex_head.Height / 2), scale / 4f, LastFacedLeft || health[head] <= 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);

			foreach (Attack a in attacks)
				a.Draw(sb, this.color);

			// Debug
//			DrawLine(sb, MainGame.tex_blank, 2, Color.Cyan, groundSensorStart * MainGame.METER_TO_PIXEL, groundSensorEnd * MainGame.METER_TO_PIXEL);
//			sb.DrawString(MainGame.fnt_basicFont, OnGround.ToString(), Vector2.Zero, Color.Cyan);
//			sb.DrawString(MainGame.fnt_basicFont, attackAngle.ToString(), Vector2.One * 64, Color.White); 
//			sb.DrawString(MainGame.fnt_basicFont, "L", LeftFootPosition * MainGame.METER_TO_PIXEL, Color.Blue);
//			sb.DrawString(MainGame.fnt_basicFont, "R", RightFootPosition * MainGame.METER_TO_PIXEL, Color.Lime);
//			sb.DrawString(MainGame.fnt_basicFont, leftLowerLeg.Friction.ToString(), Vector2.UnitY * 32, Color.White);
//			sb.DrawString(MainGame.fnt_basicFont, rightLowerLeg.Friction.ToString(), Vector2.UnitY * 64, Color.White);
		}

		/// <summary>
		/// Used for drawing lines for debugging
		/// </summary>
		/// <param name="batch">The SpriteBatch used to draw the line</param>
		/// <param name="blank">A white texture</param>
		/// <param name="width">The thickness of the line</param>
		/// <param name="color">The color of the line</param>
		/// <param name="point1">Start point of line draw</param>
		/// <param name="point2">End point of line draw</param>
		private void DrawLine(SpriteBatch batch, Texture2D blank,
			  float width, Color color, Vector2 point1, Vector2 point2)
		{
			float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
			float length = Vector2.Distance(point1, point2);

			batch.Draw(blank, point1, null, color,
					   angle, Vector2.Zero, new Vector2(length, width),
					   SpriteEffects.None, 0);
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
