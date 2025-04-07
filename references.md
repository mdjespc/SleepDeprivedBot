MONGODB for NoSQL schemas

Example Schema Elements (General):

* Users Table/Collection:
`user_id` (String/Number - Discord User ID)
`username` (String)
`discriminator` (String)
`level` (Integer)
`experience` (Integer)
`currency` (Integer)

* Servers Table/Collection:
`server_id` (String/Number - Discord Server ID)
`prefix` (String - Custom command prefix)
`language` (String - Bot language)
`welcome_channel` (String - Channel ID)

* Commands Table/Collection:
`command_name` (String)
`command_description` (String)
`command_usage` (String)
`command_enabled` (Boolean)

Audit Log Table/Collection:
