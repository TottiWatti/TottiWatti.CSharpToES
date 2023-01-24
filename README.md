# CSharpToES
 C# class models to ES (ECMAScript) class models converter with JSDoc comment / type definition support using Roslyn code analysis

## General
CSharpToES is a .NET Core console application to convert C# poco classes to ECMAScript classes with JSDoc comments and type definitions to use with JavaScript for example in ASP.NET Core program. Roslyn code analysis is used to parse code C# files.
Application uses config file for setting C# source folder and ECMAScript destination folder. Source folder can contain multiple files and subfolders and output files are arranged with same structure.

## Supported properties
CSharpToES supports following properties:
* Property values protection by using private fields for data and getter/setter for accessing it so that for example properties at ES side can be nulled only if they are nullable at C# side
* C# comments to JSDoc comments conversion
* Initialization values of native values for example to create similar data both C# and ES side with new keyword
* Single level inheritance
* Arrays, lists and dictionaries.Map objects are used for dictionaries at ES side
* Enums
* Custom JSON serializers and deserializers. Because data is stored in private fields at ES side, which are not serialized or deserialized by standard JSON parse and stringify
* Numeric range attibutes to limit setting range on both sides
	
## Setup and debugging
Download project and open in Visual Studio 2022. Project includes TestInput folder with test C# models to convert when debugging from Visual Studio.

## Usage
After project is compiled generate exe can be used for example from ASP.NET Core. Program should then be called externally and provide input folder (C# models) and output folder (ES models) as parameters.
If project is, for example, saved to local folder C:\CSharpToES and compiled in debug mode and source C# model folder is C:\MyCsProject\Models and ES model output folder is C:\MyWebProject\js\Models
conversion can be triggered from console by following command
```
C:\CSharpToES\TottiWatti.CSharpToES\bin\Debug\net6.0\CSharpToES.exe C:\MyCsProject\Models C:\MyWebProject\js\Models
```
Compiled folder with CSharpToES.exe can of course be copied to easier local path.
When used, for example, from ASP.NET Core project where C# models and ES models are within same solution source and target folders can be further automated to be independent of web project's local folder by adding conversion call in project's pre-build event like following
```
C:\CSharpToES\TottiWatti.CSharpToES\bin\Debug\net6.0\CSharpToES.exe $(ProjectDir)Shared $(ProjectDir)wwwroot\src\shared 
```

## Conversion format
CSharpToES makes opionated c# to js model conversion. Simpliest way to convert C# model to ES model would be just blindly create properties with same name as in C# model at ES class constructor. 
Standard js serialization and deserialization would be work out of the box between models. However this way properties are not protected as nothing would prevent js code to write variable of any type and any value to property.
Therefore CSharpToES uses local variables in ES models that are accessed with getters and setters. This way it can be controlled that js code cannot write just anything to property.
Let's see conversion result by example (test models included in project).
First there is WeatherForecast structure
```C#
using System.ComponentModel.DataAnnotations;

namespace EsSpaTemplate.Shared
{
    /// <summary>
    /// Weather forecast class definition
    /// </summary>
    public record struct WeatherForecast
    {
        public WeatherForecast()
        {

        }

        /// <summary>
        /// Forecast date time
        /// </summary>
        public DateTime Date { get; set; } = DateTime.Now;

        [Range(-50, 100)]
        /// <summary>
        /// Forecast tempereture in celsius degress
        /// </summary>
        public int TemperatureC { get; set; } = 0;

        [Range(-58, 212)]
        /// <summary>
        /// Forecast temperature in fahrenheit degrees
        /// </summary>
        public int TemperatureF { get; set; } = 32;       

        /// <summary>
        /// Forecast summary enum value
        /// </summary>
        public WeatherForecastSummary Summary { get; set; } = WeatherForecastSummary.Cool;        
    }
}
```
And then enum definition used by WeatherForecast in another file at the same folder (could be subfolder also)
```C#
namespace EsSpaTemplate.Shared
{
    /// <summary>
    /// Forecast feel enum definition
    /// </summary>
    public enum WeatherForecastSummary
    {
        /// <summary>
        /// Feels freezing
        /// </summary>
        Freezing,
        
        /// <summary>
        /// Feels bracing
        /// </summary>
        Bracing, 
        
        /// <summary>
        /// Feels chilly
        /// </summary>
        Chilly, 
        
        /// <summary>
        /// Feels cool
        /// </summary>
        Cool, 
        
        /// <summary>
        /// Feels mild
        /// </summary>
        Mild, 
        
        /// <summary>
        /// Feels warm
        /// </summary>
        Warm, 
        
        /// <summary>
        /// Feels balmy
        /// </summary>
        Balmy, 
        
        /// <summary>
        /// Feels hot
        /// </summary>
        Hot, 
        
        /// <summary>
        /// Feels sweltering
        /// </summary>
        Sweltering, 
        
        /// <summary>
        /// Feels scorching
        /// </summary>
        Scorching
    }
}
```
Now there is lot of unnecessary comments but they are there for demonstration reason.
As structure and enum definitions are in separate files they will be in separate files also at conversion results.
Conversion result of WeatherForecast struct would be 
```javascript
import { WeatherForecastSummary } from './WeatherForecastSummary.js';

/** Weather forecast class definition */
export class WeatherForecast {

    // private values
    /** @type {Date} */ #Date;
    /** @type {number} */ #TemperatureC;
    /** @type {number} */ #TemperatureF;
    /** @type {WeatherForecastSummary} */ #Summary;

    /** Weather forecast class definition */
    constructor() {
        this.#Date = new Date();
        this.#TemperatureC = 0;
        this.#TemperatureF = 32;
        this.#Summary = WeatherForecastSummary.Cool;
    }

    /**
    * Forecast date time
    * Server type 'DateTime'
    * @type {Date}
    */
    get Date() {
        return this.#Date;
    }
    set Date(val) {
        if (val instanceof Date) {
            this.#Date = val;
        }
    }

    /**
    * Server type 'int' custom range -50 ...  100
    * @type {number}
    */
    get TemperatureC() {
        return this.#TemperatureC;
    }
    set TemperatureC(val) {
        if (typeof val === 'number') {
            this.#TemperatureC = (val < -50 ? -50 : (val >  100 ?  100 : Math.round(val)))
        }
    }

    /**
    * Server type 'int' custom range -58 ...  212
    * @type {number}
    */
    get TemperatureF() {
        return this.#TemperatureF;
    }
    set TemperatureF(val) {
        if (typeof val === 'number') {
            this.#TemperatureF = (val < -58 ? -58 : (val >  212 ?  212 : Math.round(val)))
        }
    }

    /**
    * Forecast summary enum value
    * Server type enum 'WeatherForecastSummary' values [0,1,2,3,4,5,6,7,8,9]
    * @type {WeatherForecastSummary}
    */
    get Summary() {
        return this.#Summary;
    }
    set Summary(val) {
        if ([0,1,2,3,4,5,6,7,8,9].includes(val)) {
            this.#Summary = val;
        }
    }

    /** WeatherForecast JSON serializer. Called automatically by JSON.stringify(). */
    toJSON() {
        return {
            'Date': this.#Date,
            'TemperatureC': this.#TemperatureC,
            'TemperatureF': this.#TemperatureF,
            'Summary': this.#Summary
        }
    }

    /**
    * Deserializes json to instance of WeatherForecast.
    * @param {string} json json serialized WeatherForecast instance
    * @returns {WeatherForecast} deserialized WeatherForecast class instance
    */
    static fromJSON(json) {
        let o = JSON.parse(json);
        return WeatherForecast.fromObject(o);
    }

    /**
    * Maps object to instance of WeatherForecast.
    * @param {object} o object to map instance of WeatherForecast from
    * @returns {WeatherForecast} mapped WeatherForecast class instance
    */
    static fromObject(o) {
        if (o != null) {
            let val = new WeatherForecast();
            if (o.hasOwnProperty('Date')) { val.Date = new Date(o.Date); }
            if (o.hasOwnProperty('TemperatureC')) { val.TemperatureC = o.TemperatureC; }
            if (o.hasOwnProperty('TemperatureF')) { val.TemperatureF = o.TemperatureF; }
            if (o.hasOwnProperty('Summary')) { val.Summary = o.Summary; }
            return val;
        }
        return null;
    }

    /**
    * Deserializes json to array of WeatherForecast.
    * @param {string} json json serialized WeatherForecast array
    * @returns {WeatherForecast[]} deserialized WeatherForecast array
    */
    static fromJSONArray(json) {
        let arr = JSON.parse(json);
        return WeatherForecast.fromObjectArray(arr);
    }

    /**
    * Maps array of objects to array of WeatherForecast.
    * @param {object[]} arr object array to map WeatherForecast array from
    * @returns {WeatherForecast[]} mapped WeatherForecast array
    */
    static fromObjectArray(arr) {
        if (arr != null) {
            let /** @type {WeatherForecast[]} */ val = [];
            arr.forEach(function (f) { val.push(WeatherForecast.fromObject(f)); });
            return val;
        }
        return null;
    }

}
```
and conversion result of WeatherForecastSummary enum would be 
```javascript
/**
* Forecast feel enum definition
* @readonly
* @enum {number}
* @property {number} Freezing Feels freezing
* @property {number} Bracing Feels bracing
* @property {number} Chilly Feels chilly
* @property {number} Cool Feels cool
* @property {number} Mild Feels mild
* @property {number} Warm Feels warm
* @property {number} Balmy Feels balmy
* @property {number} Hot Feels hot
* @property {number} Sweltering Feels sweltering
* @property {number} Scorching Feels scorching
*/
export const WeatherForecastSummary = {
    /** Feels freezing */
    Freezing: 0,
    /** Feels bracing */
    Bracing: 1,
    /** Feels chilly */
    Chilly: 2,
    /** Feels cool */
    Cool: 3,
    /** Feels mild */
    Mild: 4,
    /** Feels warm */
    Warm: 5,
    /** Feels balmy */
    Balmy: 6,
    /** Feels hot */
    Hot: 7,
    /** Feels sweltering */
    Sweltering: 8,
    /** Feels scorching */
    Scorching: 9
}
```
There is a lot of js code generated, I know, but there is a reason behind it. 
All C# comments are compiled to JSDoc equivalents along with type defintions so they are available in JavaScript intellisense. 
Properties are set to same default values as in C# model so creating new instance of model in JavaScript will have same values as default new instance in C#. 
Range attributes are supported so if range is defined in C# model property value of ES model object can be set at JavaScript only within range. If range is not defined it is generated by limits of C# variable type.
Enum values also can be set only to defined enum values at JavaScript. As JavaScript does not have enum type it is compiled to close equivalent.
Downside of protecting variable values in ES class private fields is that they cannot be used with stardard JavaScript serialization/deserialization. Therefore custom serializer and deserializer functions are generated to ES model.
Serialization function 'toJSON' is called automatically by js so it does not require any extra coding in js. Deserialization however must be done through provided functions. 'fromJSON' deserialises single ES class instance from json data.
If json data contains array model instances 'fromJSONArray' should be used. 'fromObject' can be used to map standard js object to ES model object and 'fromObjectArray' can be used to map array of standard js objects to array of ES model objects.




