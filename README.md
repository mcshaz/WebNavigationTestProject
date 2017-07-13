# WebNavigationTestProject
## An example of how to implement custom INavigationNodePermissionResolver for cloudscribe.Web.Navigation;

The project uses 
* The cloudscribe.Web.Navigation library
* A navigation.xml (can easily be .json) file describing the website tree (but with no information as to what policy or roles are required in order fot the request principal to access the action)
* The Authorize filters adorning controllers and actions, including mixtures of role based and custom policy based arguments
In order to display only the nodes a user can access in any menus/sitemaps/breadcrums etc.

The explanation of this project is available on the [cloudscribe.Web.Navigation wiki](https://github.com/joeaudette/cloudscribe.Web.Navigation/wiki/Using-Authorize-Attributes-to-Implement-Menu-Filtering)
