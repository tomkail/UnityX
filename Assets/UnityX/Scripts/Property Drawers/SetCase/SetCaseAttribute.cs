using UnityEngine;

public class SetCaseAttribute : PropertyAttribute {
	
	public enum CaseType {
		Upper,
		Lower
	}
	
	public CaseType caseType;
	
	public SetCaseAttribute (CaseType caseType) {
		this.caseType = caseType;
	}
}