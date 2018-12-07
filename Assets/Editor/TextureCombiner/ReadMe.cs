// Texture Combiner V_0.1
//
// Permets de combiner différentes textures en une seule. (Roughness dans l'alpha de la metallic ou autre..)
//
// Le channel R de l'outil sert de références. Si le G, B ou A sont laissés vides, ils seront remplis avec les channels de la texture de référence.
// Les champs inutiles peuvent étre laissés vide.
// 
// Crée une nouvelle texture à coté de celle dans le channel R de la forme "nomDeLaTextureR_combined" si nom de la nouvelle texture est laissé tel que.
// Si une texture avec le meme nom a déja été crée, celle-ci est écrasée.

// IMPORTANT : - Ne sauvegarde quand TGA pour le moment.
//			   - Les textures doivent avoir EXACTEMENT les memes dimensions (2048*2048, 2048*1024).
//			   - "EncodeToTGAExtension" n'est pas de moi, merci de respecter les conditions si vous récupérez le script.
