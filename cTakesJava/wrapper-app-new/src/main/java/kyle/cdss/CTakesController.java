package kyle.cdss;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class CTakesController {

    @GetMapping("/")
    public String index() {
        return "Java wrapper working for CDSS!";
    }

}
