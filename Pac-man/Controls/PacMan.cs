﻿using System.Linq;
using System.Drawing;
using Pac_man.Classes;

namespace Pac_man.Controls
{
	public delegate void PacmanMovement(object sender, System.Drawing.Point location);
	public delegate void PacmanPointsChanged(object sender, int totalPoints);
	public delegate void PacmanMessages(object sender, string messages);

	public sealed class Pacman : Character, ICharacter
	{
		public event PacmanMovement PacmanMovement;
		public event PacmanPointsChanged PacmanPointsChanged;
		public event PacmanMessages PacmanMessages;
		
		#region Fields, Props

		private readonly Dots[] _dots = null;

		private bool _catched = false;

		public bool[,] AllowedLocationsMap { get; set; }

		MovementWay _movement = MovementWay.Right;

		public Point Exit;

		#endregion 

		#region Constructors

		public Pacman()
		{
			this.Width = this.Height = 20;
			this.PacmanMovement += Pacman_Pacman_Movement;
			Exit = new Point();
		}

		public Pacman(Dots[] dots, bool[,] allowedMapPlaces)
			: this()
		{
			_dots = dots;
			AllowedLocationsMap = allowedMapPlaces;
		}
		#endregion
		
		void Pacman_Pacman_Movement(object sender, System.Drawing.Point location)
		{
			for (int i = 0; i <= _dots.Length - 1; i++)
			{
				if (_dots[i] == null)
					continue;

				if (_dots[i].Location.X >= location.X && 
					_dots[i].Location.X <= (location.X + (this.Width / 3)) && 
					_dots[i].Location.Y >= location.Y && 
					_dots[i].Location.Y <= ((this.Height / 3) + location.Y))
				{
					Pacman pacman = sender as Pacman;
					if (pacman != null) pacman.TotalPoints += _dots[i].Points;
					_dots[i].Dispose();
					_dots[i] = null;
				}
			}

			if ((_dots.Count(d => d != null) < 1))
			{
				// if all points collected - unblock exit;
				AllowedLocationsMap[Exit.X/20, Exit.Y/20-1] = false;
				if (this.Location == new Point(Exit.X, Exit.Y))
				{
					if (PacmanMessages != null)
						PacmanMessages(this, "You win !!");
				}
			}
		}
		
		private bool IsAllowed(MovementWay move)
		{
			bool result = true;

			Point loc = new Point();
			loc.X = this.Location.X / 20;
			loc.Y = this.Location.Y / 20 - 1;

			switch (move)
			{
				case MovementWay.Right:
					{
						loc.X += 1;
						break;
					}
				case MovementWay.Left:
					{
						loc.X -= 1;
						break;
					}

				case MovementWay.Up:
					{
						loc.Y -= 1;
						break;
					}
				case MovementWay.Down:
					{
						loc.Y += 1;
						break;
					}
			}
			if (loc.X >= 29 || loc.X < 0 || loc.Y >= 29 || loc.Y < 0)
			{
				return false;
			}

			if (this.AllowedLocationsMap[loc.X, loc.Y])
			{
				result = false;
			}

			return result;
		}
	
		public void Catched(Enemy sender)
		{
			Graphics g = this.CreateGraphics();

			g.FillEllipse(System.Drawing.Brushes.Red, 0, 0, Width, Height);
			g.FillEllipse(System.Drawing.Brushes.Black, 20, 10, 5, 5);
			g.FillEllipse(System.Drawing.Brushes.Transparent, 35, 20, 10, 5);


			_catched = true;

			if (PacmanMessages != null)
				PacmanMessages(this, "Pacman has been catched by an enemy.");
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			DrawCharacter.Draw(ref e, Type);
			base.OnPaint(e);
		}

		public CharacterType Type { get { return CharacterType.Packman; }set{}}

		public override void Move(MovementWay way)
		{
			if (_catched)
				return;

			_movement = way;

			if (!IsAllowed(_movement))
			{
				return;
			}

			base.Move(way);
			
			if (_dots != null)
			{
				PacmanMovement(this, this.Location);
				return;
			}

			if (PacmanMovement != null)
				PacmanMovement(this, this.Location);

		}

	    int _totalPoints = 0;
		public int TotalPoints
		{
			get
			{
				return _totalPoints;
			}
			set
			{
				_totalPoints = value;
				if (PacmanPointsChanged != null)
					PacmanPointsChanged(this, value);
			}
		}
	}

}
