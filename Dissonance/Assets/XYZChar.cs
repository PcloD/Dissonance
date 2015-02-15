using UnityEngine;
using System.Collections;

public class XYZChar : WorldEntity {
	[SerializeField]
	WorldEntity2D _xyComponent;
	[SerializeField]
	WorldEntity2D _zyComponent;

	public virtual bool CastsShadows {
		get { return false; }
	}

	public override void Simulate () {

	}
}
