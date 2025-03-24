using TMPro;

namespace BrushingAndLinking
{
    public enum ParticantButton
    {
        More,
        Less
    }

    public class ParticipantInfoButton : ButtonGroupChild
    {
        public ParticantButton buttonMode;
        public TextMeshPro participantID;
        public override void Select()
        {
            base.Select();
            
            switch (buttonMode)
            {
                case ParticantButton.More:
                    StudyManager.Instance.CurrentParticipantID++;
                    break;
                case ParticantButton.Less:
                    StudyManager.Instance.CurrentParticipantID--;
                    break;
            }

            int pID = StudyManager.Instance.CurrentParticipantID;
            participantID.text = pID.ToString();
        }
    }
}