# Basic command format

|Command|Target selector|Additional parameters|
|:---:|:---:|:---:|
|download|=127.0.0.1:1234|"C:\\Users\\User\\Desktop\\Something Interesting.txt"|

# Target selector
|Name|Prefix (mnemonic)|Example|
|:---|:---:|---:|
|All|`*`|`*` always selects all availble connected clients|
|Equals|`=`|`=127.0.0.1:1337` selects only '127.0.01:1337'|
|Contains|`~`|`~192.` selects '<u>192.</u>123.256.145:1337', '231.<u>192.</u>132.251:1337', etc.|
|StartsWith|`[`|`[15.` selects '<u>15.</u>23.235.36:643', '<u>15.</u>251.251.12:7863', etc.|
|EndsWith|`]`|`]1:1337` selects '63.74.234.4<u>1:1337</u>', '<u>52.63.76.13<u>1:1337</u>', etc.|
|Regex|`/`|`/127.[\d]{2}.[\d]{2}.1:1[1|2|3]{3}7` selects '127.<u>12</u>.<u>23</u>.1:1<u>321</u>7', etc.|

Prefixes not listed here are considered as 'Equals(`~`)'


# Command List

## Global

## InfoCollector

### `env` - Environment variable list dump
|`env`|Target selector|(Optional) save to specified file||
|:---:|:---:|:---:|---:|
|`env`|=127.0.0.1:1234||Prints the list of environment variables of client '127.0.0.1:1234'|
|`pslist`|=127.0.0.1:1234|C:\\Logs\\Env.log|Environment variable list of '127.0.0.1:1234' saved to `C:\Logs\Env.log`|


### `pslist` - Process list dump
|`pslist`|Target selector|(Optional) save to specified file||
|:---:|:---:|:---:|---:|
|`pslist`|=127.0.0.1:1234||Prints the list of processes of client '127.0.0.1:1234'|
|`pslist`|=127.0.0.1:1234|C:\\Logs\\Processes.log|Process list of '127.0.0.1:1234' saved to `C:\Logs\Processes.log`|

