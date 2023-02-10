using Character;

namespace Interface
{
	public interface IInteractable
	{
		void OnCanInteract(CharacterBase characterBase);
		void OnInteract(CharacterBase characterBase);
		void OnEndInteract(CharacterBase characterBase);
	}
}