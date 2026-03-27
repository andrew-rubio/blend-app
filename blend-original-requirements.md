# **Blend App Summary**

**SETUP**

* On first app opening, there is an app splash intro to explain to the user what the app does  
  * User can find this again from App settings  
* User can be registered  
  * by Email (requires Name, Email, Password)  
  * or by logging in with social media accounts (FB/Google/Twitter)  
* User can do “forgot your password” to send reset email  
* User can set preferences to personalise their app experience  
  * Favourite cuisines and dish types  
  * Diet preferences or intolerances  
  * Ingredients they can’t eat or don’t like (searchable)  
  * These user preferences can be edited again in app settings  
  * Preferences will alter app suggestions (recipes & ingredients)

**HOME**

* User can search for recipes based on keywords or ingredients  
* Featured recipes at the top, which are app/admin-determined (e.g. by chefs/restaurants)  
* There will be featured stories, mini blog posts on particular topics, featured on the homepage and created weekly or bi-weekly  
* “Your community’s recipes” will be visible if the user have friends on the app, where the user can see their latest recipes  
* “Recently viewed” recipes included  
* There are mini videos on cooking techniques available on the app (to be provided in-house, e.g. by working chefs)

**COOK**

* User can decide to cook without following a recipe in the app  
* When cooking, user first chooses cuisine or dish type so the app knows which ingredients to suggest  
* User can search for ingredients to add to their dish  
* The app should suggest ingredients that compliment the flavours of the current dish  
  * This suggestion list should update when new ingredients are added to the dish by the user, or if ingredients are removed from the dish  
  * The suggestion list should have ingredients ordered by how complementary they are to the current list of ingredients  
    * The data for the ingredient’s “complimentary” strength will be provided initially in bulk, but can also be taken from APIs \- it should also be updated on-the-go based on user feedback after cooking a meal  
  * User can filter the suggested ingredients by filter tags  
  * Ingredients are clickable to show more info on an ingredient: larger image, description, showcase why it goes with the other ingredients (from database), nutritional info and substitute ingredients (also clickable)  
  * User can add suggested ingredients to their dish  
* User can edit dish notes  
* User can add a new dish to create multiple dishes at the same time, which the user can swipe between. The user should also be able to remove a dish.  
* User can edit the name of the dish  
* When a user has finished cooking with their own dish, they can mark which ingredients stood out positively or negatively, add notes, photos (and select which should be featured as the main photo), category tags, add to MyFitnessPal, or share via social media platforms.  
  * The ingredient ratings will influence the strength in relationship between ingredient A and ingredient B for that dish/cuisine type  
* Option to post recipe publically or just save to profile privately  
  * If posting as a recipe, it will require information like prep time, cook time, servings, description, amounts per ingredient listed, directions (with ability to add image/video for any step)  
* User can also “Find recipes” from their list of ingredients  
* A user can also “cook” a recipe that they find on the explore page or when searching for recipes  
  * When cooking a recipe from the app, it will take them to a similar UI (to be defined)

**EXPLORE**

* User can view Trending recipes  
* User can view “Recommended for you” recipes  
  * Based on preferences and recent activity  
  * These can be filtered by the tags available  
* User can search for recipes by keywords or ingredients  
* When viewing a recipe, there are 3 tabs: Overview, Ingredients, Directions  
  * Overview: prep time, cooking time, servings, description, nutritional info, photos and reviews  
  * Ingredients: amounts per ingredient based on servings (which is changeable), list of ingredients which are clickable to see more info on ingredient  
  * Directions: step by step (possibility to include in-line pictures or videos  
  * Ability to click “cook this dish” to be taken to cooking screen  
  * User can add to MyFitnessPal  
  * User can share on social platforms  
  * User can “like” recipe to profile

**PROFILE**

* There is a profile section where system will allow users to search for friends and add friends  
* User can “search” through their own recipes and liked recipes  
  * Own recipes can be either set to private (only viewable by user) or public  
    * Liked recipes are those that the user has “liked” from the Explore section or from friends profiles  
  * For a profile that a user is visiting, they can only see “My recipes” which are public, and “Liked recipes”  
* User can edit profile: name and photo  
* User will be able to like recipes of other users they are following or any featured recipes on their home/feed (e.g. from restaurants or chefs)  
* For new users, the recommendations of recipes are shown by default by the system based on the preferences they set when signing up (recipes shown would be from trending chefs/restaurants for example)  
* User can go to app settings  
  * User can submit a new ingredient to the app  
  * App can be shared to friends via usual sharing channels (WhatsApp, SMS, Facebook, etc.)  
  * User can change units between imperial or metric  
  * Ability to see full list of ingredients used in the app