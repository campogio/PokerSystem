using System;
using GTANetworkAPI;

namespace SouthValleyFive.data.persistent
{
	public class PokerTableSit
		{
			public int Id { get; set; }
			public Vector3 Position { get; set; }
			public Vector3 Rotation { get; set; }
			public Player Occupied { get; set; }
			public GTANetworkAPI.Object Model { get; set; }

			public PokerTableSit(int id, Vector3 position, Vector3 rotation)
			{
				Id = id;
				Position = position;
				Rotation = rotation;
				Occupied = null;

				Model = NAPI.Object.CreateObject(2353589337, new Vector3(position.X, position.Y, position.Z - 1), new Vector3(rotation.X,rotation.Y,rotation.Z+180));

			}
	}
}