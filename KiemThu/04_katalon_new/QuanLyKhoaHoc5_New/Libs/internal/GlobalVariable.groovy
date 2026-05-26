package internal

import com.kms.katalon.core.configuration.RunConfiguration
import com.kms.katalon.core.main.TestCaseMain


/**
 * This class is generated automatically by Katalon Studio and should not be modified or deleted.
 */
public class GlobalVariable {
     
    /**
     * <p>Profile default : Base URL c&#7911;a &#7913;ng d&#7909;ng web</p>
     */
    public static Object BASE_URL
     
    /**
     * <p>Profile default : Email t&agrave;i kho&#7843;n Admin</p>
     */
    public static Object ADMIN_EMAIL
     
    /**
     * <p>Profile default : M&#7853;t kh&#7849;u t&agrave;i kho&#7843;n Admin</p>
     */
    public static Object ADMIN_PASS
     
    /**
     * <p>Profile default : Th&#432; m&#7909;c ch&#7913;a file Excel test data</p>
     */
    public static Object TEST_DATA_DIR
     
    /**
     * <p>Profile default : Timeout ch&#7901; element (gi&acirc;y)</p>
     */
    public static Object TIMEOUT
     

    static {
        try {
            def selectedVariables = TestCaseMain.getGlobalVariables('default')
			selectedVariables += TestCaseMain.getGlobalVariables(RunConfiguration.getExecutionProfile())
    
            BASE_URL = selectedVariables['BASE_URL']
            ADMIN_EMAIL = selectedVariables['ADMIN_EMAIL']
            ADMIN_PASS = selectedVariables['ADMIN_PASS']
            TEST_DATA_DIR = selectedVariables['TEST_DATA_DIR']
            TIMEOUT = selectedVariables['TIMEOUT']
            
        } catch (Exception e) {
            TestCaseMain.logGlobalVariableError(e)
        }
    }
}
